import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { NotificationService } from '../../services/notification.service';
import Swal from 'sweetalert2';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './main-layout.html',
  styleUrls: ['./main-layout.scss'],
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  roles: string[] = [];
  username: string = '';
  notifCount: number = 0;
  private notifInterval: any;

  searchQuery: string = '';

  navItems = [
    { label: 'dashboard', route: '/dashboard' },
    { label: 'enrollments', route: '/admin/enrollments', role: 'Admin' },
    { label: 'batches', route: '/batches', role: 'Admin' },
    { label: 'assignments', route: '/assignments', role: 'Admin' },
    { label: 'qna admin', route: '/qna-admin', role: 'Admin' },
    { label: 'attendance admin', route: '/attendance-admin', role: 'Admin' },
    { label: 'meetings admin', route: '/meetings-admin', role: 'Admin' },
    { label: 'manage documents', route: '/manage-documents', role: 'Admin' },

    { label: 'courses', route: '/courses', role: 'Intern' },
    { label: 'qna', route: '/qna', role: 'Intern' },
    { label: 'attendance', route: '/attendance', role: 'Intern' },
    { label: 'upload documents', route: '/upload-documents', role: 'Intern' },
    { label: 'meetings', route: '/meetings', role: 'Intern' },
    { label: 'notifications', route: '/notifications', role: 'Intern' },
  ];

  private jwtHelper = new JwtHelperService();

  constructor(
    private auth: AuthService,
    private router: Router,
    private notifService: NotificationService
  ) {}

  ngOnInit(): void {
    this.roles = JSON.parse(localStorage.getItem('roles') || '[]');

    
    const token = localStorage.getItem('token');
    if (token) {
      const decoded = this.jwtHelper.decodeToken(token);
      this.username = decoded['unique_name'] || decoded['username'] || 'User';
    } else {
      this.username = 'User';
    }

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
      error: () => (this.notifCount = 0),
    });
  }

  logout() {
    Swal.fire({
      title: 'Are you sure?',
      text: 'You will be logged out of your account.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, logout',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.auth.logout();
        this.router.navigate(['/login']);
      }
    });
  }

  isAdmin(): boolean {
    return this.roles.includes('Admin');
  }

  isIntern(): boolean {
    return this.roles.includes('Intern');
  }

  onSearch() {
    const query = this.searchQuery.toLowerCase().trim();
    if (!query) return;

    const matches = this.navItems.filter(
      (item) =>
        item.label.toLowerCase().includes(query) &&
        (!item.role || this.roles.includes(item.role))
    );

    if (matches.length > 0) {
      this.router.navigate([matches[0].route]);
    } else {
      Swal.fire('No matching page found', '', 'info');
    }

    this.searchQuery = '';
  }
}
