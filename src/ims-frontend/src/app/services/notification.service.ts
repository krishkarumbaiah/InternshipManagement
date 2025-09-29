// src/app/services/notification.service.ts
import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subscription, timer } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../environments/environment';

export interface NotificationDto {
  id: any;
  message: string;
  meetingId?: any;
  meetingLink?: string;
  scheduledAt?: string | null;
  notifyAt?: string | null;
  batchName?: string;
  isSent?: boolean;
  createdAt?: string | null;
  // ... keep raw fields if backend returns additional props
  [k: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private apiUrl = environment.apiUrl;
  private notifCountSubject = new BehaviorSubject<number>(0);
  public notifCount$ = this.notifCountSubject.asObservable();

  // optional caching of the last notifications list
  private notificationsCache: NotificationDto[] = [];

  // auto-refresh subscription (optional)
  private refreshSub?: Subscription;

  constructor(private http: HttpClient) {
    // initial load of the count
    this.refreshCount();

    // auto refresh count every 60 seconds - adjust as desired
    this.refreshSub = timer(60000, 60000).subscribe(() => this.refreshCount());
  }

  /** Get all notifications for the logged-in intern */
  getMyNotifications(): Observable<NotificationDto[]> {
    const url = `${this.apiUrl}/notifications/mine`;
    return this.http.get<NotificationDto[]>(url).pipe(
      map((res: any) => Array.isArray(res) ? res : []),
      tap((list) => {
        this.notificationsCache = list;
        // keep the count in sync when explicitly fetching notifications
        this.notifCountSubject.next(list.length);
      }),
      catchError((err) => {
        console.error('NotificationService.getMyNotifications error', err);
        // keep existing cache, but return empty for UI safety
        return of([] as NotificationDto[]);
      })
    );
  }

  /**
   * Refresh only the notification count (lightweight)
   * This calls backend and counts returned items.
   */
  refreshCount(): void {
    const url = `${this.apiUrl}/notifications/mine`;
    this.http.get<any[]>(url).pipe(
      map(list => (Array.isArray(list) ? list.length : 0)),
      catchError(err => {
        console.error('NotificationService.refreshCount failed', err);
        return of(0);
      })
    ).subscribe(count => this.notifCountSubject.next(count));
  }

  /** Optionally expose last fetched notifications (sync) */
  getCachedNotifications(): NotificationDto[] {
    return this.notificationsCache;
  }

  ngOnDestroy(): void {
    if (this.refreshSub) this.refreshSub.unsubscribe();
  }
}
