import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-meetings-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './meetings-admin.html',
  styleUrls: ['./meetings-admin.scss']
})
export class MeetingsAdminComponent implements OnInit {
  meetings: any[] = [];
  batches: any[] = [];
  newMeeting = { title: '', description: '', scheduledAt: '', meetingLink: '', batchId: '' };

  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadMeetings();
    this.loadBatches();
  }

  loadMeetings() {
    this.http.get<any[]>(`${environment.apiUrl}/meetings`).subscribe({
      next: (res) => (this.meetings = res),
      error: () => this.toastr.error('Failed to load meetings')
    });
  }

  loadBatches() {
    this.http.get<any[]>(`${environment.apiUrl}/batches`).subscribe({
      next: (res) => (this.batches = res),
      error: () => this.toastr.error('Failed to load batches')
    });
  }

  createMeeting() {
    if (!this.newMeeting.batchId) {
      this.toastr.warning('Please select a batch');
      return;
    }

    this.http.post(`${environment.apiUrl}/meetings`, this.newMeeting).subscribe({
      next: () => {
        this.toastr.success('Meeting created ✅');
        this.newMeeting = { title: '', description: '', scheduledAt: '', meetingLink: '', batchId: '' };
        this.loadMeetings();
      },
      error: () => this.toastr.error('Failed to create meeting ❌')
    });
  }

  deleteMeeting(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This meeting will be permanently deleted!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel'
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${environment.apiUrl}/meetings/${id}`).subscribe({
          next: () => {
            this.toastr.success('Meeting deleted successfully 🗑️');
            this.loadMeetings();
          },
          error: () => this.toastr.error('Failed to delete meeting ❌'),
        });
      }
    });
  }

  // ✅ Sorting function
  sortTable(column: string) {
    if (this.sortColumn === column) {
      // toggle direction
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.meetings.sort((a, b) => {
      const valA = a[column];
      const valB = b[column];

      if (valA == null) return 1;
      if (valB == null) return -1;

      if (typeof valA === 'string') {
        return this.sortDirection === 'asc'
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      } else {
        return this.sortDirection === 'asc'
          ? valA > valB ? 1 : -1
          : valA < valB ? 1 : -1;
      }
    });
  }
}
