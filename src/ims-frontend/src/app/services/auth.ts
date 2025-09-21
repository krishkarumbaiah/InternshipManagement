import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, data);
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register`, data);
  }

  //  Send OTP
  sendOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/send-otp`, JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // Forgot Password
  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/forgot-password`, { email });
  }

  // Reset Password
  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/reset-password`, {
      email,
      token,
      newPassword
    });
  }

  saveAuthData(token: string, roles: string[]) {
    localStorage.setItem('token', token);
    localStorage.setItem('roles', JSON.stringify(roles));
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRoles(): string[] {
    const roles = localStorage.getItem('roles');
    return roles ? JSON.parse(roles) : [];
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.getRoles().includes('Admin');
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('roles');
  }
}
