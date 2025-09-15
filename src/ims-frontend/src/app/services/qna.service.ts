import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class QnaService {
  private apiUrl = `${environment.apiUrl}/qna`;

  constructor(private http: HttpClient) {}

  // Intern: get my questions
  getMyQuestions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my`);
  }

  // Intern: ask new question
  askQuestion(text: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/ask`, { text });
  }

  // Admin: get all questions
  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // Admin: answer a question
  answerQuestion(id: number, answer: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/answer/${id}`, { answer });
  }
}
// Admin: delete a question