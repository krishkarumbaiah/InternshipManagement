import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-qna-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qna-admin.html',
  styleUrls: ['./qna-admin.scss'],
})
export class QnaAdminComponent implements OnInit {
  qna: any[] = [];
  apiUrl = environment.apiUrl;

  // 🔹 For edit mode
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
    if (!confirm('Are you sure you want to delete this question?')) return;

    this.http.delete(`${this.apiUrl}/qna/${id}`).subscribe({
      next: () => {
        this.toastr.info('Question deleted');
        this.loadQuestions();
      },
      error: () => this.toastr.error('Failed to delete question'),
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
