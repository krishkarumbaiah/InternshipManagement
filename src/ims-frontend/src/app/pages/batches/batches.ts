import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';  
import { BatchService } from '../../services/batch';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-batches',
  standalone: true,
  templateUrl: './batches.html',
  styleUrls: ['./batches.scss'],
  imports: [CommonModule, FormsModule]   
})
export class BatchesComponent implements OnInit {
  batches: any[] = [];
  newBatch: any = { name: '', startDate: '', endDate: '' };
  editBatch: any = null;

  constructor(private batchService: BatchService, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches() {
    this.batchService.getBatches().subscribe({
      next: (data) => this.batches = data,
      error: () => this.toastr.error('Failed to load batches')
    });
  }

  addBatch() {
    this.batchService.createBatch(this.newBatch).subscribe({
      next: () => {
        this.toastr.success('Batch added successfully');
        this.loadBatches();
        this.newBatch = { name: '', startDate: '', endDate: '' };
      },
      error: () => this.toastr.error('Failed to add batch')
    });
  }

  startEdit(batch: any) {
    this.editBatch = { ...batch };
  }

  updateBatch() {
    this.batchService.updateBatch(this.editBatch.id, this.editBatch).subscribe({
      next: () => {
        this.toastr.success('Batch updated successfully');
        this.loadBatches();
        this.editBatch = null;
      },
      error: () => this.toastr.error('Failed to update batch')
    });
  }

  deleteBatch(id: number) {
    if (confirm('Are you sure you want to delete this batch?')) {
      this.batchService.deleteBatch(id).subscribe({
        next: () => {
          this.toastr.success('Batch deleted successfully');
          this.loadBatches();
        },
        error: () => this.toastr.error('Failed to delete batch')
      });
    }
  }
}
