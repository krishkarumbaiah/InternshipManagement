import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface Announcement {
  id: number;
  title: string;
  message: string;
  createdAt: string;
  batchName: string;
}

@Injectable({
  providedIn: 'root',
})
export class AnnouncementService {
  private apiUrl = `${environment.apiUrl}/announcements`;

  constructor(private http: HttpClient) {}

  // Admin: create announcement
  createAnnouncement(data: { title: string; message: string; batchId: number }): Observable<any> {
    return this.http.post(`${this.apiUrl}`, data);
  }

  // Intern: get my announcements
  getMyAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/my`);
  }
  // Admin: get all announcements
  getAllAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}`);
  }

  // Admin: delete an announcement
  deleteAnnouncement(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
