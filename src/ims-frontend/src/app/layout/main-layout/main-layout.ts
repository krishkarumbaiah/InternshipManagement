import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { NotificationService } from '../../services/notification.service';
import Swal from 'sweetalert2';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Subscription } from 'rxjs';

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
  profilePhoto: string | null = null;
  notifCount: number = 0;

  private notifInterval: any;
  private photoSub?: Subscription;
  private notifSub?: Subscription;

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
    { label: 'manage leaves', route: '/leave-manage', role: 'Admin' },
    { label: 'feedback', route: '/feedback-admin', role: 'Admin' },
    { label: 'announcements', route: '/announcements/create', role: 'Admin' },

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
    // roles + username as before
    this.roles = JSON.parse(localStorage.getItem('roles') || '[]');
    const token = localStorage.getItem('token');
    if (token) {
      const decoded = this.jwtHelper.decodeToken(token);
      this.username = decoded['unique_name'] || decoded['username'] || 'User';
    } else {
      this.username = 'User';
    }

    // --- 1) First try to show local storage value immediately (fast) ---
    const initial = this.auth.getProfilePhoto();
    if (initial) {
      this.profilePhoto = initial;
      console.log('MainLayout: using initial profilePhoto from AuthService/localStorage ->', initial);
    }

    // --- 2) Ensure we load latest profile from API (guarantee fresh value) ---
    // If the user is logged in, call backend to get latest profile (this avoids stale localStorage)
    if (this.auth.isLoggedIn()) {
      this.auth.getProfile().subscribe({
        next: (res) => {
          if (res?.profilePhoto) {
            // AuthService.setProfilePhoto will attach timestamp and push subject
            this.auth.setProfilePhoto(res.profilePhoto);
            console.log('MainLayout: fetched profile from API and set global photo:', res.profilePhoto);
          } else {
            // ensure default if none
            this.auth.setProfilePhoto('/assets/default-avatar.png');
          }
        },
        error: (err) => {
          console.warn('MainLayout: failed to fetch profile on init', err);
        }
      });
    }

    // --- 3) Subscribe to BehaviorSubject to receive updates after edits ---
this.photoSub = this.auth.profilePhoto$.subscribe(photo => {
  if (!photo) { this.profilePhoto = null; return; }
  // add cache-buster only if not present
  this.profilePhoto = photo.includes('?') ? photo : `${photo}?t=${Date.now()}`;
});

    // notif subscription and polling
    if (this.isIntern()) {
      this.notifSub = this.notifService.notifCount$.subscribe((count) => (this.notifCount = count));
      this.notifService.refreshCount();
      this.notifInterval = setInterval(() => this.notifService.refreshCount(), 60000);
    }
  }

  ngOnDestroy(): void {
    if (this.notifInterval) clearInterval(this.notifInterval);
    if (this.photoSub) this.photoSub.unsubscribe();
    if (this.notifSub) this.notifSub.unsubscribe();
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
        item.label.toLowerCase().includes(query) && (!item.role || this.roles.includes(item.role))
    );

    if (matches.length > 0) {
      this.router.navigate([matches[0].route]);
    } else {
      Swal.fire('No matching page found', '', 'info');
    }

    this.searchQuery = '';
  }
}
