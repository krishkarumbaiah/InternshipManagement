import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '../../services/attendance.service';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-attendance-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attendance-admin.html',
  styleUrls: ['./attendance-admin.scss']
})
export class AttendanceAdminComponent implements OnInit {
  batches: any[] = [];
  users: any[] = [];
  selectedBatch: number | null = null;
  date: string = new Date().toISOString().split('T')[0];
  attendance: { [userId: number]: boolean } = {};

  constructor(
    private http: HttpClient,
    private attendanceService: AttendanceService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches() {
    this.http.get<any[]>(`${environment.apiUrl}/batches`).subscribe({
      next: (res) => (this.batches = res),
      error: () => this.toastr.error('Failed to load batches')
    });
  }

  loadUsers(batchId: number | null) {
    if (!batchId) return; // if null, exit
    this.http.get<any[]>(`${environment.apiUrl}/userbatches/bybatch/${batchId}`).subscribe({
      next: (res) => (this.users = res),
      error: () => this.toastr.error('Failed to load users for batch')
    });
  }


  markAttendance() {
    if (!this.selectedBatch) {
      this.toastr.warning('Please select a batch first');
      return;
    }

    this.users.forEach((u) => {
      this.attendanceService
        .markAttendance({
          userId: u.userId,
          batchId: this.selectedBatch,
          date: this.date,
          isPresent: this.attendance[u.userId] || false
        })
        .subscribe({
          next: () => this.toastr.success(`Attendance marked for ${u.userName}`),
          error: () => this.toastr.error(`Failed to mark ${u.userName}`)
        });
    });
  }
}
