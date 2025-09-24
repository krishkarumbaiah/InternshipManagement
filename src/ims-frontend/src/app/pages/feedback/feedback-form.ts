import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { FeedbackService } from '../../services/feedback.service';

@Component({
  selector: 'app-feedback-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './feedback-form.html',
  styleUrls: ['./feedback-form.scss']
})
export class FeedbackFormComponent {
  feedbackForm: FormGroup;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private feedbackSvc: FeedbackService,
    private toastr: ToastrService
  ) {
    this.feedbackForm = this.fb.group({
      comments: ['', [Validators.required, Validators.maxLength(1000)]],
      rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]]
    });
  }

  // getters for template-friendly and type-safe access
  get comments() { return this.feedbackForm.get('comments'); }
  get rating()   { return this.feedbackForm.get('rating'); }

  submitFeedback() {
    if (this.feedbackForm.invalid) {
      this.toastr.warning('Please complete the form');
      this.feedbackForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const payload = this.feedbackForm.value;

    this.feedbackSvc.submit(payload).subscribe({
      next: () => {
        this.toastr.success('Thanks — your feedback was submitted');
        this.feedbackForm.reset({ comments: '', rating: 5 });
        this.submitting = false;
      },
      error: (err) => {
        console.error('Feedback submit error', err);
        this.toastr.error(err?.error?.message || 'Failed to submit feedback');
        this.submitting = false;
      }
    });
  }
}
