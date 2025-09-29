import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../environments/environment';
import { AuthService } from '../services/auth';
import Swal from 'sweetalert2';

interface Leave {
  id: number;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
}

@Component({
  selector: 'app-leave-apply',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './leave-apply.html',
  styleUrls: ['./leave-apply.scss'],
})
export class LeaveApplyComponent implements OnInit {
  leaveForm: FormGroup;
  leaves: Leave[] = [];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private toastr: ToastrService,
    private auth: AuthService
  ) {
    this.leaveForm = this.fb.group({
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      reason: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadMyLeaves();
  }

  applyLeave() {
    if (!this.leaveForm.valid) return;

    const data = this.leaveForm.value;

    this.http.post(`${environment.apiUrl}/leave/apply`, data).subscribe({
      next: () => {
        this.toastr.success('Leave applied successfully!');
        this.leaveForm.reset();
        this.loadMyLeaves();
      },
      error: (err) => {
        console.error('Leave apply error:', err);
        if (err.status === 401) {
          this.toastr.error('Session expired. Please login again.');
          this.auth.logout();
        } else {
          this.toastr.error(err.error?.message || 'Failed to apply leave');
        }
      },
    });
  }

  loadMyLeaves() {
    this.http.get<Leave[]>(`${environment.apiUrl}/leave/my-leaves`).subscribe({
      next: (res) => (this.leaves = res),
      error: (err) => {
        console.error('Load my leaves error:', err);
        this.toastr.error('Failed to load leave history');
      },
    });
  }

  cancelLeave(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'Do you really want to cancel this leave request?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, cancel it',
      cancelButtonText: 'No, keep it',
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${environment.apiUrl}/leave/cancel/${id}`).subscribe({
          next: () => {
            Swal.fire('Cancelled!', 'Your leave request has been cancelled.', 'success');
            this.loadMyLeaves();
          },
          error: (err) => {
            console.error('Cancel leave error:', err);
            Swal.fire('Error!', err.error?.message || 'Failed to cancel leave', 'error');
          },
        });
      }
    });
  }
}
