import { Component, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  username: string = '';
  roles: string[] = [];
  overview: any = null;
  myOverview: any = null; // ✅ Intern-specific data

  private jwtHelper = new JwtHelperService();

  constructor(
    private auth: AuthService,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const token = this.auth.getToken();
    if (token) {
      const decoded = this.jwtHelper.decodeToken(token);
      this.username = decoded['unique_name'] || decoded['username'];
      this.roles = Array.isArray(decoded['role']) ? decoded['role'] : [decoded['role']];
    }

    if (this.isAdmin()) {
      this.loadAdminOverview();
    } else if (this.isIntern()) {
      this.loadInternOverview();
    }
  }

  isAdmin(): boolean {
    return this.roles.includes('Admin');
  }

  isIntern(): boolean {
    return this.roles.includes('Intern');
  }

  loadAdminOverview() {
    this.http.get<any>(`${environment.apiUrl}/reports/overview`).subscribe({
      next: (res) => (this.overview = res),
      error: () => console.error('Failed to load admin dashboard data')
    });
  }

  loadInternOverview() {
    this.http.get<any>(`${environment.apiUrl}/reports/myoverview`).subscribe({
      next: (res) => (this.myOverview = res),
      error: () => console.error('Failed to load intern dashboard data')
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
