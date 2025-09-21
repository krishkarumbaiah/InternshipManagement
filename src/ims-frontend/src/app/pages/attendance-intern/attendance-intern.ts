import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';
import { PaginatorModule } from 'primeng/paginator';
import { PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-attendance-intern',
  standalone: true,
  imports: [CommonModule, PaginatorModule],
  templateUrl: './attendance-intern.html',
  styleUrls: ['./attendance-intern.scss']
})
export class AttendanceInternComponent implements OnInit {
  attendance: any[] = [];

  
  page: number = 0;   
  rows: number = 5;

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadMyAttendance();
  }

  loadMyAttendance() {
    this.http.get<any[]>(`${environment.apiUrl}/attendance/my`).subscribe({
      next: (res) => (this.attendance = res),
      error: () => this.toastr.error('Failed to load attendance')
    });
  }

  onPageChange(event: PaginatorState) {
    this.page = event.page ?? 0;
    this.rows = event.rows ?? 5;
  }
  get pagedAttendance() {
    const start = this.page * this.rows;
    return this.attendance.slice(start, start + this.rows);
  }
}
