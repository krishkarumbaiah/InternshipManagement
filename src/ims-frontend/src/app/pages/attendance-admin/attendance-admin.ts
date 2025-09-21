import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '../../services/attendance.service';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-attendance-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attendance-admin.html',
  styleUrls: ['./attendance-admin.scss'],
  encapsulation: ViewEncapsulation.None  
})
export class AttendanceAdminComponent implements OnInit {
  batches: any[] = [];
  users: any[] = [];
  selectedBatch: number | null = null;
  date: string = new Date().toISOString().split('T')[0];
  attendance: { [userId: number]: boolean } = {};
  history: any[] = [];

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
    if (!batchId) return;
    this.http.get<any[]>(`${environment.apiUrl}/userbatches/bybatch/${batchId}`).subscribe({
      next: (res) => {
        this.users = res;
        this.loadHistory(batchId);  
      },
      error: () => this.toastr.error('Failed to load users for batch')
    });
  }

  loadHistory(batchId: number) {
    this.http.get<any[]>(`${environment.apiUrl}/attendance/bybatch/${batchId}`).subscribe({
      next: (res) => (this.history = res),
      error: () => this.toastr.error('Failed to load attendance history')
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
          batchId: this.selectedBatch!,
          date: this.date,
          isPresent: this.attendance[u.userId] || false
        })
        .subscribe({
          next: () => {
            this.toastr.success(`Attendance marked for ${u.userName}`);
            this.loadHistory(this.selectedBatch!);  
          },
          error: () => this.toastr.error(`Failed to mark ${u.userName}`)
        });
    });
  }

  deleteHistory(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This attendance record will be permanently deleted!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel'
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${environment.apiUrl}/attendance/${id}`).subscribe({
          next: () => {
            this.toastr.success('Attendance record deleted ✅');
            if (this.selectedBatch) this.loadHistory(this.selectedBatch);
          },
          error: () => this.toastr.error('Failed to delete record ❌')
        });
      }
    });
  }

  
  exportToExcel() {
    const worksheet = XLSX.utils.json_to_sheet(this.history);
    const workbook = { Sheets: { data: worksheet }, SheetNames: ['data'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });
    saveAs(blob, `Attendance_History.xlsx`);
  }


  exportToPDF() {
    const doc = new jsPDF();
    doc.text('📊 Attendance History', 14, 10);
    autoTable(doc, {
      head: [['#', 'Intern', 'Batch', 'Status', 'Date']],
      body: this.history.map((h, i) => [
        i + 1,
        h.userName,
        h.batchName,
        h.isPresent ? 'Present ✅' : 'Absent ❌',
        new Date(h.date).toLocaleDateString()
      ])
    });
    doc.save('Attendance_History.pdf');
  }
}
