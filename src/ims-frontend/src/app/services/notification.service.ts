// src/app/services/notification.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;

  // ✅ Real-time notification count observable
  private notifCountSubject = new BehaviorSubject<number>(0);
  notifCount$ = this.notifCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get current logged-in user's notifications.
   */
  getMyNotifications(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/mine`);
  }

  /**
   * Admin: Get all notifications.
   */
  getAllNotifications(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  /**
   * ✅ Refresh count and push to subject.
   */
  refreshCount(): void {
    this.getMyNotifications().subscribe({
      next: (res) => this.notifCountSubject.next(res?.length ?? 0),
      error: () => this.notifCountSubject.next(0),
    });
  }
}
