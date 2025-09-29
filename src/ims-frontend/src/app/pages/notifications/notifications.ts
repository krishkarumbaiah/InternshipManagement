import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';
import { interval, Subscription } from 'rxjs';

interface NotificationItem {
  id: any;
  message: string;
  meetingId: any;
  meetingLink: string;
  scheduledAt: Date | null;
  notifyAt: Date | null;
  batchName: string;
  isSent: boolean;
  createdAt: Date | null;
  raw: any;
}

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  styleUrls: ['./notifications.scss'],
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications: NotificationItem[] = [];
  loading = false;
  private refreshSub?: Subscription;
  private clockSub?: Subscription;

  now: Date = new Date();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadNotifications();

    // Reload every 60s
    this.refreshSub = interval(60000).subscribe(() => this.loadNotifications());

    // Update "now" every 30s
    this.clockSub = interval(30000).subscribe(() => (this.now = new Date()));
  }

  ngOnDestroy(): void {
    if (this.refreshSub) this.refreshSub.unsubscribe();
    if (this.clockSub) this.clockSub.unsubscribe();
  }

  loadNotifications() {
    this.loading = true;
    this.now = new Date();

    this.notificationService.getMyNotifications().subscribe({
      next: (res) => {
        const raw = Array.isArray(res) ? res : [];

      
        const normalized = raw
          .map((item: any) => this.normalizeNotification(item))
          .filter(this.isValidMeetingNotification) 
          .sort((a, b) => {
            
            const aTime = (a.notifyAt ?? a.createdAt ?? new Date(0)).getTime();
            const bTime = (b.notifyAt ?? b.createdAt ?? new Date(0)).getTime();
            return bTime - aTime;
          });

        this.notifications = normalized;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load notifications', err);
        this.notifications = [];
        this.loading = false;
      },
    });
  }

 
  private normalizeNotification(item: any): NotificationItem | null {
    if (!item) return null;

    const meetingId =
      item.meetingId ?? item.MeetingId ?? item.MeetingID ?? item.meeting_id ?? null;
    const meetingLink =
      item.meetingLink ?? item.MeetingLink ?? item.meeting_link ?? item.meetingUrl ?? '';
    const scheduledAtVal =
      item.scheduledAt ?? item.ScheduledAt ?? item.scheduled_at ?? item.scheduledAtLocal ?? null;
    const notifyAtVal = item.notifyAt ?? item.NotifyAt ?? item.notify_at ?? null;
    const batchName = item.batchName ?? item.BatchName ?? item.batch_name ?? '';
    const createdAt = item.createdAt ?? item.CreatedAt ?? null;

    
    const scheduledAt = this.parseDateSafe(scheduledAtVal);
    const notifyAt = this.parseDateSafe(notifyAtVal);
    const created = this.parseDateSafe(createdAt);

    return {
      id: item.id ?? item.Id ?? null,
      message: item.message ?? item.Message ?? '',
      meetingId,
      meetingLink,
      scheduledAt,
      notifyAt,
      batchName,
      isSent: item.isSent ?? item.IsSent ?? false,
      createdAt: created,
      raw: item,
    };
  }


  private isValidMeetingNotification(
    n: NotificationItem | null
  ): n is NotificationItem {
    if (!n) return false;

    if (n.meetingId === null || n.meetingId === undefined) return false;
    if (!(n.scheduledAt instanceof Date) || isNaN(n.scheduledAt.getTime())) return false;

    return true;
  }

  private parseDateSafe(value: any): Date | null {
    if (!value) return null;
    try {
      const d = value instanceof Date ? value : new Date(value);
      if (isNaN(d.getTime())) return null;
      return d;
    } catch {
      return null;
    }
  }

  trackById(index: number, item: NotificationItem) {
    return item.id;
  }

  isUpcoming(scheduledAt: string | Date | null | undefined): boolean {
    if (!scheduledAt) return false;
    const d = scheduledAt instanceof Date ? scheduledAt : new Date(scheduledAt);
    return !isNaN(d.getTime()) && d > this.now;
  }

  getMinutesLeft(scheduledAt: string | Date | null | undefined): number {
    if (!scheduledAt) return 0;
    const d = scheduledAt instanceof Date ? scheduledAt : new Date(scheduledAt);
    if (isNaN(d.getTime())) return 0;
    return Math.max(0, Math.round((d.getTime() - this.now.getTime()) / 60000));
  }
}
