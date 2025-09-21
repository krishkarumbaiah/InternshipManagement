import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BatchService } from '../../services/batch';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import Swal from 'sweetalert2';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-batches',
  standalone: true,
  templateUrl: './batches.html',
  styleUrls: ['./batches.scss'],
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, ToastModule],
  providers: [MessageService] 
})
export class BatchesComponent implements OnInit {
  batches: any[] = [];
  newBatch: any = { name: '', startDate: '', endDate: '' };
  editBatch: any = null;

  constructor(private batchService: BatchService, private messageService: MessageService) {}

  ngOnInit(): void {
    this.loadBatches();
  }

  loadBatches() {
    this.batchService.getBatches().subscribe({
      next: (data) => (this.batches = data),
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load batches' })
    });
  }

  addBatch() {
    this.batchService.createBatch(this.newBatch).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Batch added successfully' });
        this.loadBatches();
        this.newBatch = { name: '', startDate: '', endDate: '' };
      },
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to add batch' })
    });
  }

  startEdit(batch: any) {
    this.editBatch = { ...batch };
  }

  updateBatch() {
    this.batchService.updateBatch(this.editBatch.id, this.editBatch).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Updated', detail: 'Batch updated successfully' });
        this.loadBatches();
        this.editBatch = null;
      },
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update batch' })
    });
  }

  // Delete batch
deleteBatch(id: number) {
  Swal.fire({
    title: 'Are you sure?',
    text: 'This batch will be permanently deleted!',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#dc3545',
    cancelButtonColor: '#6c757d',
    confirmButtonText: 'Yes, delete it!',
    cancelButtonText: 'Cancel'
  }).then((result) => {
    if (result.isConfirmed) {
      this.batchService.deleteBatch(id).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Deleted',
            detail: 'Batch deleted successfully ✅'
          });
          this.loadBatches();
        },
        error: () =>
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to delete batch ❌'
          })
      });
    }
  });
}


  // Export to Excel
  exportToExcel() {
    const worksheet = XLSX.utils.json_to_sheet(this.batches);
    const workbook = { Sheets: { data: worksheet }, SheetNames: ['data'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });
    saveAs(blob, 'Batches.xlsx');
  }

  // Export to PDF
  exportToPDF() {
    const doc = new jsPDF();
    doc.text('Batches Report', 14, 10);
    autoTable(doc, {
      head: [['#', 'Batch Name', 'Start Date', 'End Date']],
      body: this.batches.map((b, i) => [
        i + 1,
        b.name,
        new Date(b.startDate).toLocaleDateString(),
        new Date(b.endDate).toLocaleDateString()
      ]),
    });
    doc.save('Batches.pdf');
  }
}
