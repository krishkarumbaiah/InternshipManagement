import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule], 
  templateUrl: './main-layout.html',
  styleUrls: ['./main-layout.scss']
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  roles: string[] = [];
  username: string = '';
  notifCount: number = 0;
  private notifInterval: any;

  constructor(
    private auth: AuthService,
    private router: Router,
    private notifService: NotificationService
  ) {}

  ngOnInit(): void {
    this.roles = JSON.parse(localStorage.getItem('roles') || '[]');
    this.username = localStorage.getItem('username') || '';

    if (this.isIntern()) {
      this.loadNotifications();
      this.notifInterval = setInterval(() => this.loadNotifications(), 60000);
    }
  }

  ngOnDestroy(): void {
    if (this.notifInterval) clearInterval(this.notifInterval);
  }

  loadNotifications() {
    this.notifService.getMyNotifications().subscribe({
      next: (res) => (this.notifCount = res?.length ?? 0),
      error: () => (this.notifCount = 0)
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  isAdmin(): boolean {
    return this.roles.includes('Admin');
  }

  isIntern(): boolean {
    return this.roles.includes('Intern');
  }
}
