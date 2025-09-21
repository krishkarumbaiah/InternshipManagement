import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-manage-documents',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, ToastModule],
  templateUrl: './manage-documents.html',
  styleUrls: ['./manage-documents.scss'],
})
export class ManageDocumentsComponent {
  documents: any[] = [];

  constructor(private http: HttpClient, private toastr: ToastrService) {
    this.loadDocuments();
  }

  loadDocuments() {
    this.http.get<any[]>(`${environment.apiUrl}/documents`).subscribe({
      next: (res) => (this.documents = res),
      error: (err) => {
        console.error('Failed to load documents', err);
        this.toastr.error('Failed to load documents');
      },
    });
  }

  downloadDocument(id: number, fileName: string) {
    this.http
      .get(`${environment.apiUrl}/documents/download/${id}`, { responseType: 'blob' })
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = fileName;
          document.body.appendChild(a);
          a.click();
          a.remove();
          window.URL.revokeObjectURL(url);
          this.toastr.success(`Downloading ${fileName}`);
        },
        error: (err) => {
          console.error('Failed to download document', err);
          this.toastr.error('Failed to download document');
        },
      });
  }

  // Delete document with SweetAlert2
  deleteDocument(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This document will be permanently deleted!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${environment.apiUrl}/documents/${id}`).subscribe({
          next: () => {
            this.toastr.success('Document deleted successfully 🗑️');
            this.loadDocuments();
          },
          error: () => this.toastr.error('Failed to delete document ❌'),
        });
      }
    });
  }
}
