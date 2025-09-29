import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormControl,
  FormGroup,
} from '@angular/forms';
import { AnnouncementService, Announcement } from '../../../services/announcement.service';
import { ToastrService } from 'ngx-toastr';
import { BatchService } from '../../../services/batch';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-announcement-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './announcement-create.html',
  styleUrls: ['./announcement-create.scss'],
})
export class AnnouncementCreateComponent implements OnInit {
  batches: any[] = [];
  announcements: Announcement[] = [];
  loading = true;

  announcementForm!: FormGroup<{
    title: FormControl<string | null>;
    message: FormControl<string | null>;
    batchId: FormControl<number | null>;
  }>;

  constructor(
    private fb: FormBuilder,
    private announcementService: AnnouncementService,
    private batchService: BatchService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.announcementForm = this.fb.group({
      title: this.fb.control<string | null>(null, Validators.required),
      message: this.fb.control<string | null>(null, Validators.required),
      batchId: this.fb.control<number | null>(null, Validators.required),
    });

    this.loadBatches();
    this.loadAnnouncements();
  }

  loadBatches() {
    this.batchService.getBatches().subscribe({
      next: (res) => {
        this.batches = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load batches', err);
        this.toastr.error('Could not load batches');
        this.loading = false;
      },
    });
  }

  loadAnnouncements() {
    this.announcementService.getAllAnnouncements().subscribe({
      next: (res) => {
        this.announcements = res;
      },
      error: () => {
        this.toastr.error('Failed to load announcements');
      },
    });
  }

  onSubmit() {
    if (this.announcementForm.valid) {
      const payload = {
        title: this.announcementForm.controls.title.value!,
        message: this.announcementForm.controls.message.value!,
        batchId: this.announcementForm.controls.batchId.value!,
      };

      this.announcementService.createAnnouncement(payload).subscribe({
        next: () => {
          this.toastr.success('Announcement created successfully!');
          this.announcementForm.reset();
          this.loadAnnouncements(); // refresh history table
        },
        error: (err) => {
          console.error(err);
          this.toastr.error('Failed to create announcement');
        },
      });
    }
  }

  deleteAnnouncement(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: 'This will permanently delete the announcement.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Yes, delete it',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (result.isConfirmed) {
        this.announcementService.deleteAnnouncement(id).subscribe({
          next: () => {
            Swal.fire('Deleted!', 'The announcement has been removed.', 'success');
            this.announcements = this.announcements.filter((a) => a.id !== id);
          },
          error: () => {
            Swal.fire('Error!', 'Failed to delete announcement.', 'error');
          },
        });
      }
    });
  }
}
