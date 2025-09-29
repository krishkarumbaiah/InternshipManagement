// src/app/services/auth.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = environment.apiUrl;

  private profilePhotoSubject = new BehaviorSubject<string | null>(this.loadInitialPhoto());
  profilePhoto$ = this.profilePhotoSubject.asObservable();

  constructor(private http: HttpClient) {}

  private loadInitialPhoto(): string | null {
   
    const userId = localStorage.getItem('userId');
    if (userId) {
      const byUser = localStorage.getItem(`profilePhoto_${userId}`);
      if (byUser && byUser.trim() !== '') return byUser;
    }

    const generic = localStorage.getItem('profilePhoto');
    if (generic && generic.trim() !== '') return generic;

    return null;
  }

  // --------- Auth / API calls ----------
  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, data);
  }

  register(data: any): Observable<any> {
    if (data instanceof FormData) {
      return this.http.post(`${this.apiUrl}/auth/register`, data);
    }
    return this.http.post(`${this.apiUrl}/auth/register`, data);
  }

  sendOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/send-otp`, JSON.stringify(email), {
      headers: { 'Content-Type': 'application/json' },
    });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/forgot-password`, { email });
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/reset-password`, {
      email,
      token,
      newPassword,
    });
  }


  saveAuthData(token: string, roles: string[] = [], userId?: string | number | null, profilePhoto?: string) {
    if (token) localStorage.setItem('token', token);
    if (roles) localStorage.setItem('roles', JSON.stringify(roles));
    if (userId !== undefined && userId !== null) {
      localStorage.setItem('userId', String(userId));
      if (profilePhoto) {
        const key = `profilePhoto_${userId}`;
        localStorage.setItem(key, profilePhoto);
        
        this.profilePhotoSubject.next(profilePhoto);
      }
    } else if (profilePhoto) {
      
      localStorage.setItem('profilePhoto', profilePhoto);
      this.profilePhotoSubject.next(profilePhoto);
    }
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRoles(): string[] {
    const roles = localStorage.getItem('roles');
    return roles ? JSON.parse(roles) : [];
  }

  getUserId(): number | null {
    const id = localStorage.getItem('userId');
    return id ? Number(id) : null;
  }


  getProfilePhoto(): string {
    const cur = this.profilePhotoSubject.getValue();
    return cur && cur.trim() !== '' ? cur : '/assets/default-avatar.png';
  }


  setProfilePhoto(photoUrl: string | null) {
    const userId = this.getUserId();

    const finalUrl = photoUrl && photoUrl.trim() !== '' ? photoUrl : '/assets/default-avatar.png';

    if (userId) {
      const key = `profilePhoto_${userId}`;
      try {
        localStorage.setItem(key, finalUrl);
      } catch (e) {
       
        console.warn('Failed to save profilePhoto to localStorage', e);
      }
    } else {
      
      try {
        localStorage.setItem('profilePhoto', finalUrl);
      } catch (e) {
        console.warn('Failed to save generic profilePhoto to localStorage', e);
      }
    }

    
    this.profilePhotoSubject.next(finalUrl);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.getRoles().includes('Admin');
  }

  getProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/profile`);
  }

  updateProfile(data: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/profile`, data);
  }

  logout() {
    const userId = this.getUserId();
    if (userId) {
      localStorage.removeItem(`profilePhoto_${userId}`);
    }
   
    localStorage.removeItem('profilePhoto');

    localStorage.removeItem('token');
    localStorage.removeItem('roles');
    localStorage.removeItem('userId');
    this.profilePhotoSubject.next(null);
  }
}
