// feedback.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface FeedbackRequest {
  comments: string;
  rating: number;
}

export interface FeedbackResponse {
  id: number;
  userName: string;
  comments: string;
  rating: number;
  submittedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  private api = environment.apiUrl;

  constructor(private http: HttpClient) {}

  submit(feedback: FeedbackRequest): Observable<any> {
    return this.http.post(`${this.api}/feedback/submit`, feedback);
  }

  getAll(): Observable<FeedbackResponse[]> {
    return this.http.get<FeedbackResponse[]>(`${this.api}/feedback/all`);
  }

  // Optional: delete feedback by admin
  delete(id: number) {
    return this.http.delete(`${this.api}/feedback/delete/${id}`);
  }
}
