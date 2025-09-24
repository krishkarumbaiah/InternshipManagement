import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../environments/environment';
import { AuthService } from '../services/auth';

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

    // ❌ don't include userId, backend uses JWT claim
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
    if (!confirm('Are you sure you want to cancel this leave request?')) return;

    this.http.delete(`${environment.apiUrl}/leave/cancel/${id}`).subscribe({
      next: () => {
        this.toastr.success('Leave request cancelled');
        this.loadMyLeaves();
      },
      error: (err) => {
        console.error('Cancel leave error:', err);
        this.toastr.error(err.error?.message || 'Failed to cancel leave');
      },
    });
  }
}
