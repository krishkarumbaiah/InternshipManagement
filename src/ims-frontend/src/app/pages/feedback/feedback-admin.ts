import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FeedbackService, FeedbackResponse } from '../../services/feedback.service';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-feedback-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './feedback-admin.html',
  styleUrls: ['./feedback-admin.scss']
})
export class FeedbackAdminComponent implements OnInit {
  feedbacks: FeedbackResponse[] = [];
  loading = false;

  constructor(private svc: FeedbackService, private toastr: ToastrService) {}

  ngOnInit() {
    this.loadFeedback();
  }

  loadFeedback() {
    this.loading = true;
    this.svc.getAll().subscribe({
      next: (res) => { this.feedbacks = res; this.loading = false; },
      error: (err) => { console.error(err); this.toastr.error('Failed to load feedback'); this.loading = false; }
    });
  }

  // optional delete function if you want admin to remove feedback
  deleteFeedback(id: number) {
    Swal.fire({
      title: 'Delete feedback?',
      text: 'This will remove the feedback permanently.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Delete'
    }).then(result => {
      if (result.isConfirmed) {
        this.svc.delete(id).subscribe({
          next: () => { Swal.fire('Deleted', 'Feedback removed', 'success'); this.loadFeedback(); },
          error: (err) => { console.error(err); Swal.fire('Error', 'Unable to delete', 'error'); }
        });
      }
    });
  }
}
