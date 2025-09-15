import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BatchService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getBatches(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/batches`);
  }

  createBatch(batch: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/batches`, batch);
  }

  updateBatch(id: number, batch: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/batches/${id}`, batch);
  }

  deleteBatch(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/batches/${id}`);
  }
}
