import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-qna',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qna.html',
  styleUrls: ['./qna.scss']
})
export class QnaComponent implements OnInit {
  newQuestion = '';
  myQuestions: any[] = [];
  allQuestions: any[] = [];
  editingQuestionId: number | null = null;
  editedText: string = '';

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadMyQuestions();
    this.loadAllQuestions();
  }

  // Load Intern's own questions
  loadMyQuestions() {
    this.http.get<any[]>(`${environment.apiUrl}/qna/my`).subscribe({
      next: (res) => (this.myQuestions = res),
      error: () => this.toastr.error('Failed to load my questions')
    });
  }

  // Load all questions (to see answers from Admin)
  loadAllQuestions() {
    this.http.get<any[]>(`${environment.apiUrl}/qna`).subscribe({
      next: (res) => (this.allQuestions = res),
      error: () => this.toastr.error('Failed to load all questions')
    });
  }

  // Ask new question
  askQuestion() {
    if (!this.newQuestion.trim()) return;
    this.http.post(`${environment.apiUrl}/qna/ask`, { text: this.newQuestion }).subscribe({
      next: () => {
        this.toastr.success('Question submitted');
        this.newQuestion = '';
        this.loadMyQuestions();
        this.loadAllQuestions();
      },
      error: () => this.toastr.error('Failed to submit question')
    });
  }

  // Start editing an existing question
  startEdit(q: any) {
    this.editingQuestionId = q.id;
    this.editedText = q.text;
  }

  // Save edited question
  saveEdit(id: number) {
    if (!this.editedText.trim()) return;
    this.http.put(`${environment.apiUrl}/qna/edit/${id}`, { text: this.editedText }).subscribe({
      next: () => {
        this.toastr.success('Question updated');
        this.editingQuestionId = null;
        this.loadMyQuestions();
      },
      error: () => this.toastr.error('Failed to update question')
    });
  }

  // Cancel editing
  cancelEdit() {
    this.editingQuestionId = null;
    this.editedText = '';
  }

 // Delete own question with SweetAlert2
deleteQuestion(id: number) {
  Swal.fire({
    title: 'Are you sure?',
    text: 'This question will be permanently deleted!',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#dc3545',
    cancelButtonColor: '#6c757d',
    confirmButtonText: 'Yes, delete it!',
    cancelButtonText: 'Cancel'
  }).then((result) => {
    if (result.isConfirmed) {
      this.http.delete(`${environment.apiUrl}/qna/${id}`).subscribe({
        next: () => {
          this.toastr.info('Question deleted 🗑️');
          this.loadMyQuestions();
          this.loadAllQuestions();
        },
        error: () => this.toastr.error('Failed to delete question ❌')
      });
    }
  });
}

}
