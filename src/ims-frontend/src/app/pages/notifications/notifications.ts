import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  styleUrls: ['./notifications.scss'],
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications: any[] = [];
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
        this.notifications = res || [];
        this.loading = false;
      },
      error: () => {
        console.error('Failed to load notifications');
        this.notifications = [];
        this.loading = false;
      },
    });
  }

  trackById(index: number, item: any) {
    return item.id;
  }

  isUpcoming(scheduledAt: string | Date): boolean {
    return new Date(scheduledAt) > this.now;
  }

  getMinutesLeft(scheduledAt: string | Date): number {
    return Math.max(
      0,
      Math.round((new Date(scheduledAt).getTime() - this.now.getTime()) / 60000)
    );
  }
}
