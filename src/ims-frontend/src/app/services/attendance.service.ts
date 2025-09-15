import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private apiUrl = `${environment.apiUrl}/attendance`;

  constructor(private http: HttpClient) {}

  markAttendance(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/mark`, data);
  }

  getByUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/byuser/${userId}`);
  }

  getByBatch(batchId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/bybatch/${batchId}`);
  }
}
