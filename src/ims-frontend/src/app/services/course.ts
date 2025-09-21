import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CourseService {
  private api = `${environment.apiUrl}/courses`;

  constructor(private http: HttpClient) {}

  // Intern
  getCourses(): Observable<any[]> {
    return this.http.get<any[]>(this.api);
  }

  getMyEnrollments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/me`);
  }

  enroll(courseId: number): Observable<any> {
    return this.http.post(`${this.api}/${courseId}/enroll`, {});
  }

  unenroll(courseId: number): Observable<any> {
    return this.http.delete(`${this.api}/${courseId}/unenroll`);
  }

  // Admin
  getAllEnrollments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/enrollments`);
  }

  getCourseEnrollments(courseId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/${courseId}/enrollments`);
  }
}
