import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../environments/environment';
import Swal from 'sweetalert2';

interface Leave {
  id: number;
  userId: number;
  userName?: string;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
}

@Component({
  selector: 'app-leave-manage',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leave-manage.html',
  styleUrls: ['./leave-manage.scss'],
  
})
export class LeaveManageComponent implements OnInit {
  leaves: Leave[] = [];

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit() {
    this.loadLeaves();
  }

  loadLeaves() {
    this.http.get<Leave[]>(`${environment.apiUrl}/leave/all`).subscribe({
      next: (res) => (this.leaves = res),
      error: (err) => {
        console.error('Load leaves error:', err);
        this.toastr.error('Failed to load leaves');
      },
    });
  }

  updateStatus(id: number, status: string) {
    this.http
      .put(`${environment.apiUrl}/leave/update-status/${id}?status=${status}`, {})
      .subscribe({
        next: () => {
          this.toastr.success(`Leave ${status}`);
          this.loadLeaves(); // refresh list
        },
        error: (err) => {
          console.error('Update leave status error:', err);
          this.toastr.error('Failed to update leave');
        },
      });
  }
  deleteLeave(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This leave request will be permanently deleted!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${environment.apiUrl}/leave/delete/${id}`).subscribe({
          next: () => {
            Swal.fire('Deleted!', 'Leave request has been deleted.', 'success');
            this.loadLeaves();
          },
          error: (err) => {
            console.error('Delete leave error:', err);
            Swal.fire('Error!', 'Failed to delete leave request.', 'error');
          },
        });
      }
    });
  }
}
