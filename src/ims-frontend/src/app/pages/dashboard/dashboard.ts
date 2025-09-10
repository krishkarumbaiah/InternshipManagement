import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { JwtHelperService } from '@auth0/angular-jwt';

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

  private jwtHelper = new JwtHelperService();

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    const token = this.auth.getToken();
    if (token) {
      const decoded = this.jwtHelper.decodeToken(token);
      this.username = decoded['unique_name'] || decoded['username'];
      this.roles = Array.isArray(decoded['role']) ? decoded['role'] : [decoded['role']];
    }
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
