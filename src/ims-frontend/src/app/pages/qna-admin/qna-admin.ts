import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';
import Swal from 'sweetalert2';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-qna-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, ToastModule],
  templateUrl: './qna-admin.html',
  styleUrls: ['./qna-admin.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class QnaAdminComponent implements OnInit {
  qna: any[] = [];
  apiUrl = environment.apiUrl;

  editAnswerId: number | null = null;
  editAnswerText: string = '';

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadQuestions();
  }

  loadQuestions() {
    this.http.get<any[]>(`${this.apiUrl}/qna`).subscribe({
      next: (res) => (this.qna = res),
      error: () => this.toastr.error('Failed to load questions'),
    });
  }

  answerQuestion(id: number, answer: string) {
    this.http.post(`${this.apiUrl}/qna/answer/${id}`, { answer }).subscribe({
      next: () => {
        this.toastr.success('Answer submitted');
        this.loadQuestions();
      },
      error: () => this.toastr.error('Failed to submit answer'),
    });
  }

  deleteQuestion(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This question will be permanently deleted!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.http.delete(`${this.apiUrl}/qna/${id}`).subscribe({
          next: () => {
            this.toastr.success('✅ Question deleted successfully');
            this.loadQuestions();
          },
          error: () => this.toastr.error('❌ Failed to delete question'),
        });
      }
    });
  }

  startEditAnswer(item: any) {
    this.editAnswerId = item.id;
    this.editAnswerText = item.answer || '';
  }

  cancelEditAnswer() {
    this.editAnswerId = null;
    this.editAnswerText = '';
  }

  updateAnswer(id: number) {
    if (!this.editAnswerText.trim()) {
      this.toastr.warning('Answer cannot be empty');
      return;
    }

    this.http.post(`${this.apiUrl}/qna/answer/${id}`, { answer: this.editAnswerText }).subscribe({
      next: () => {
        this.toastr.success('Answer updated');
        this.editAnswerId = null;
        this.editAnswerText = '';
        this.loadQuestions();
      },
      error: () => this.toastr.error('Failed to update answer'),
    });
  }
}
