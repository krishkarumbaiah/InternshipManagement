import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

// Chart.js imports
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule,  BaseChartDirective],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  username: string = '';
  roles: string[] = [];
  overview: any = null;
  myOverview: any = null;

  private jwtHelper = new JwtHelperService();

  // Charts
  adminUsersChart: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [] };
  adminAttendanceChart: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  internAttendanceChart: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [] };
  internQnaChart: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

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
      next: (res) => {
        this.overview = res;

        // Users chart
        this.adminUsersChart = {
          labels: ['Total Users'],
          datasets: [
            { data: [res.users.total], backgroundColor: ['#007bff'] }
          ]
        };

        // Attendance chart
        this.adminAttendanceChart = {
          labels: ['Present %', 'Absent %'],
          datasets: [
            {
              data: [res.attendance.presentRate, 100 - res.attendance.presentRate],
              backgroundColor: ['#28a745', '#dc3545']
            }
          ]
        };
      },
      error: () => console.error('Failed to load admin dashboard data')
    });
  }

  loadInternOverview() {
    this.http.get<any>(`${environment.apiUrl}/reports/myoverview`).subscribe({
      next: (res) => {
        this.myOverview = res;

        // Attendance chart
        this.internAttendanceChart = {
          labels: ['Present Days', 'Absent Days'],
          datasets: [
            {
              data: [res.attendance.presentDays, res.attendance.totalDays - res.attendance.presentDays],
              backgroundColor: ['#17a2b8', '#ffc107']
            }
          ]
        };

        // QnA chart
        this.internQnaChart = {
          labels: ['Answered', 'Unanswered'],
          datasets: [
            {
              data: [res.questions.answered, res.questions.unanswered],
              backgroundColor: ['#28a745', '#dc3545']
            }
          ]
        };
      },
      error: () => console.error('Failed to load intern dashboard data')
    });
  }
}
