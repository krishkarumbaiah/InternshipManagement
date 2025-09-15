import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-attendance-intern',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './attendance-intern.html',
  styleUrls: ['./attendance-intern.scss']
})
export class AttendanceInternComponent implements OnInit {
  attendance: any[] = [];

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
}
