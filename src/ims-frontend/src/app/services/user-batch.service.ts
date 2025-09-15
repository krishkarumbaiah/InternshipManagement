import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserBatchService {
  private apiUrl = `${environment.apiUrl}/userbatches`;

  constructor(private http: HttpClient) {}

  // Assign user to batch
  assign(userId: string, batchId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/assign`, { userId, batchId });
  }

  // Get all users in a batch
  getByBatch(batchId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/bybatch/${batchId}`);
  }

  // Get all batches for a user
  getByUser(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/byuser/${userId}`);
  }

  // Unassign user from batch
  unassign(userBatchId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/unassign/${userBatchId}`);
  }
}
