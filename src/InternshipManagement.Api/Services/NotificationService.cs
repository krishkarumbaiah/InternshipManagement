using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IServiceProvider services, ILogger<NotificationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✅ Notification Service started.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _services.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        // Time references
                        var nowUtc = DateTime.UtcNow;
                        var nowLocal = DateTime.Now;

                        // 🔹 Step 1: Create notifications for meetings happening within next 15 mins (UTC comparisons)
                        var upcomingMeetings = await db.Meetings
                            .Include(m => m.Batch)
                                .ThenInclude(b => b.UserBatches)
                                    .ThenInclude(ub => ub.User)
                            .Where(m => m.ScheduledAt > nowUtc && m.ScheduledAt <= nowUtc.AddMinutes(15))
                            .ToListAsync(stoppingToken);

                        foreach (var meeting in upcomingMeetings)
                        {
                            bool exists = await db.Notifications
                                .AnyAsync(n => n.MeetingId == meeting.Id && n.BatchId == meeting.BatchId, stoppingToken);

                            if (!exists)
                            {
                                // compute notify time in UTC (meeting.ScheduledAt is stored in UTC)
                                var notifyUtc = meeting.ScheduledAt.AddMinutes(-10);
                                if (notifyUtc < nowUtc) notifyUtc = nowUtc; 

                                // prepare user-visible local time string
                                var scheduledLocal = meeting.ScheduledAt.ToLocalTime();
                                var scheduledLocalStr = scheduledLocal.ToString("g"); // short/general

                                // store message with local time for consistent display in emails/UI
                                var message = $"Reminder: Meeting '{meeting.Title}' starts at {scheduledLocalStr}";

                                db.Notifications.Add(new Models.Notification
                                {
                                    MeetingId = meeting.Id,
                                    BatchId = meeting.BatchId,
                                    Message = message,
                                    IsSent = false,
                                    NotifyAt = notifyUtc, // stored as UTC
                                    CreatedAt = DateTime.UtcNow
                                });

                                _logger.LogInformation(
                                    "📌 Reminder scheduled for Meeting {MeetingId} | Scheduled(UTC)={ScheduledUtc:O} | Scheduled(Local)={ScheduledLocal:g} | NotifyAt(UTC)={NotifyUtc:O} | NotifyAt(Local)={NotifyLocal:g}",
                                    meeting.Id,
                                    meeting.ScheduledAt,
                                    scheduledLocal,
                                    notifyUtc,
                                    notifyUtc.ToLocalTime());
                            }
                        }

                        // 🔹 Step 2: Send due reminders (NotifyAt <= nowUtc, not sent)
                        var dueNotifications = await db.Notifications
                            .Include(n => n.Meeting)
                                .ThenInclude(m => m.Batch)
                                    .ThenInclude(b => b.UserBatches)
                                        .ThenInclude(ub => ub.User)
                            .Where(n => !n.IsSent && n.NotifyAt <= nowUtc)
                            .ToListAsync(stoppingToken);

                        foreach (var notif in dueNotifications)
                        {
                            // mark as sent (store in UTC)
                            notif.IsSent = true;

                            // Ensure the message contains a local-time formatted scheduled time (override for consistency)
                            if (notif.Meeting != null)
                            {
                                var scheduledLocal = notif.Meeting.ScheduledAt.ToLocalTime();
                                notif.Message = $"Reminder: Meeting '{notif.Meeting.Title}' starts at {scheduledLocal:g}";
                            }

                            if (notif.Meeting?.Batch?.UserBatches != null)
                            {
                                foreach (var ub in notif.Meeting.Batch.UserBatches)
                                {
                                    var email = ub.User?.Email;
                                    if (string.IsNullOrEmpty(email))
                                    {
                                        _logger.LogWarning("⚠️ Skipped user {UserId} (no email).", ub.UserId);
                                        continue;
                                    }

                                    _logger.LogInformation("📧 Sending reminder for Meeting {MeetingId} to {Email} (NotifyAt UTC: {NotifyAtUtc:O}, Local: {NotifyAtLocal:g})",
                                        notif.MeetingId,
                                        email,
                                        notif.NotifyAt,
                                        notif.NotifyAt.ToLocalTime());

                                    try
                                    {
                                        // Build a clear HTML email body with local time and link
                                        var scheduledLocal = notif.Meeting?.ScheduledAt.ToLocalTime();
                                        var scheduledLocalStr = scheduledLocal?.ToString("f"); // e.g., "Tuesday, 23 September 2025 3:42 PM"

                                        var emailBody = $@"
                                            <p>Hi {ub.User?.FullName ?? "Intern"},</p>
                                            <p>{notif.Message}.</p>
                                            <p><strong>When:</strong> {scheduledLocalStr}</p>
                                            <p><strong>Link:</strong> <a href='{notif.Meeting?.MeetingLink}'>{notif.Meeting?.MeetingLink}</a></p>
                                            <p>— IMS System</p>
                                        ";

                                        await emailService.SendEmailAsync(
                                            email,
                                            "Meeting Reminder",
                                            emailBody
                                        );

                                        _logger.LogInformation("✅ Reminder sent to {Email} for Meeting {MeetingId}", email, notif.MeetingId);
                                    }
                                    catch (Exception emailEx)
                                    {
                                        _logger.LogError(emailEx, "❌ Failed to send reminder to {Email} for Meeting {MeetingId}", email, notif.MeetingId);
                                    }
                                }
                            }

                            _logger.LogInformation("📢 Notification {NotificationId} completed for Meeting {MeetingId}", notif.Id, notif.MeetingId);
                        }

                        // 🔹 Save changes only if something changed
                        if (upcomingMeetings.Any() || dueNotifications.Any())
                        {
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("⏹ NotificationService loop canceled (shutdown).");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Unexpected error in NotificationService loop");
                    }

                    // Run every 1 minute
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("⏹ NotificationService stopped.");
            }
        }
    }
}
