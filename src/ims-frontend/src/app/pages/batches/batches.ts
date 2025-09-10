import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-batches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './batches.html',
  styleUrls: ['./batches.scss']
})
export class BatchesComponent implements OnInit {
  batches: any[] = [];
  newBatch = { name: '', startDate: '', endDate: '' };

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches() {
    this.http.get<any[]>(`${environment.apiUrl}/batches`).subscribe({
      next: (data) => (this.batches = data),
      error: () => this.toastr.error('Failed to load batches')
    });
  }

  addBatch() {
    if (!this.newBatch.name.trim() || !this.newBatch.startDate || !this.newBatch.endDate) {
      this.toastr.warning('All fields are required');
      return;
    }

    this.http.post(`${environment.apiUrl}/batches`, this.newBatch).subscribe({
      next: () => {
        this.toastr.success('Batch added successfully');
        this.newBatch = { name: '', startDate: '', endDate: '' };
        this.loadBatches();
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('Failed to add batch');
      }
    });
  }
}
