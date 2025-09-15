import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  styleUrls: ['./notifications.scss']
})
export class NotificationsComponent implements OnInit {
  notifications: any[] = [];
  loading = false;
  private refreshSub?: Subscription;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadNotifications();

    
    this.refreshSub = interval(60000).subscribe(() => this.loadNotifications());
  }

  ngOnDestroy(): void {
    if (this.refreshSub) this.refreshSub.unsubscribe();
  }

  loadNotifications() {
    this.loading = true;
    this.notificationService.getMyNotifications().subscribe({
      next: (res) => {
        this.notifications = res;
        this.loading = false;
      },
      error: () => {
        console.error('Failed to load notifications');
        this.loading = false;
      }
    });
  }

  trackById(index: number, item: any) {
    return item.id;
  }
}
