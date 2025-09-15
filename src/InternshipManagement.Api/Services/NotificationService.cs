using InternshipManagement.Api.Data;
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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var now = DateTime.UtcNow;

                        // 🔹 Step 1: Create notifications for upcoming meetings (within 15 mins)
                        var upcomingMeetings = await db.Meetings
                            .Where(m => m.ScheduledAt > now && m.ScheduledAt <= now.AddMinutes(15))
                            .ToListAsync(stoppingToken);

                        foreach (var meeting in upcomingMeetings)
                        {
                            bool exists = await db.Notifications
                                .AnyAsync(n => n.MeetingId == meeting.Id, stoppingToken);

                            if (!exists)
                            {
                                db.Notifications.Add(new Models.Notification
                                {
                                    MeetingId = meeting.Id,
                                    Message = $"Reminder: Meeting '{meeting.Title}' starts at {meeting.ScheduledAt:g}.",
                                    IsSent = false,
                                    NotifyAt = meeting.ScheduledAt.AddMinutes(-15),
                                    CreatedAt = DateTime.UtcNow
                                });

                                _logger.LogInformation($"📌 Notification created for Meeting {meeting.Id}");
                            }
                        }

                        // 🔹 Step 2: Dispatch due notifications (NotifyAt <= now)
                        var dueNotifications = await db.Notifications
                            .Where(n => !n.IsSent && n.NotifyAt <= now)
                            .ToListAsync(stoppingToken);

                        foreach (var notif in dueNotifications)
                        {
                            notif.IsSent = true;
                            _logger.LogInformation($"📢 Notification {notif.Id} marked as sent for Meeting {notif.MeetingId}");
                        }

                        // Save only if anything changed
                        if (upcomingMeetings.Any() || dueNotifications.Any())
                        {
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in NotificationService");
                }

                // Run every 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
