import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { UserBatchService } from '../../services/user-batch.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './assignments.html',
  styleUrls: ['./assignments.scss']
})
export class AssignmentsComponent implements OnInit {
  users: any[] = [];
  batches: any[] = [];
  assignments: any[] = [];

  selectedUser: string = '';
  selectedBatch: number | null = null;

  private apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private userBatchService: UserBatchService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadBatches();
  }

  // ✅ Load users from /api/users
  loadUsers() {
    this.http.get<any[]>(`${this.apiUrl}/users`).subscribe({
      next: (res) => (this.users = res),
      error: () => this.toastr.error('Failed to load users')
    });
  }

  // ✅ Load batches from /api/batches
  loadBatches() {
    this.http.get<any[]>(`${this.apiUrl}/batches`).subscribe({
      next: (res) => (this.batches = res),
      error: () => this.toastr.error('Failed to load batches')
    });
  }

  // ✅ Load assignments for selected batch
  loadAssignments(batchId: number) {
    this.userBatchService.getByBatch(batchId).subscribe({
      next: (res) => (this.assignments = res),
      error: () => this.toastr.error('Failed to load assignments')
    });
  }

  // ✅ Assign user
  assignUser() {
    if (!this.selectedUser || !this.selectedBatch) {
      this.toastr.warning('Please select a user and a batch');
      return;
    }

    this.userBatchService.assign(this.selectedUser, this.selectedBatch).subscribe({
      next: () => {
        this.toastr.success('User assigned successfully');
        this.loadAssignments(this.selectedBatch!);
      },
      error: (err) => {
        this.toastr.error(err.error || 'Failed to assign user');
      }
    });
  }

  // ✅ Unassign user
  unassignUser(userBatchId: number) {
    this.userBatchService.unassign(userBatchId).subscribe({
      next: () => {
        this.toastr.info('User unassigned');
        if (this.selectedBatch) {
          this.loadAssignments(this.selectedBatch);
        }
      },
      error: () => this.toastr.error('Failed to unassign user')
    });
  }
}
