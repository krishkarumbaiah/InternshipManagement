import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-upload-documents',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-documents.html',
  styleUrls: ['./upload-documents.scss']
})
export class UploadDocumentsComponent {
  selectedFile: File | null = null;
  myDocuments: any[] = [];

  constructor(private http: HttpClient, private toastr: ToastrService) {
    this.loadMyDocuments();
  }

  // Select file
  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  // Upload file
  upload() {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post(`${environment.apiUrl}/documents/upload`, formData).subscribe({
      next: () => {
        this.toastr.success('File uploaded successfully 🎉');
        this.loadMyDocuments();
        this.selectedFile = null;
      },
      error: () => {
        this.toastr.error('Upload failed ❌');
      }
    });
  }

  // Load my documents
  loadMyDocuments() {
    this.http.get<any[]>(`${environment.apiUrl}/documents/my`).subscribe({
      next: (res) => (this.myDocuments = res),
      error: () => this.toastr.error('Failed to load documents ❌')
    });
  }

  // Delete with SweetAlert2
  deleteDocument(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This document will be permanently deleted!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel'
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${environment.apiUrl}/documents/my/${id}`).subscribe({
          next: () => {
            this.toastr.info('Document deleted 🗑️');
            this.loadMyDocuments();
          },
          error: () => this.toastr.error('Failed to delete document ❌')
        });
      }
    });
  }
}
