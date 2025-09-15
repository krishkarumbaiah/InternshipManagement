import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-meetings-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './meetings-admin.html',
  styleUrls: ['./meetings-admin.scss']
})
export class MeetingsAdminComponent implements OnInit {
  meetings: any[] = [];
  newMeeting = { title: '', description: '', scheduledAt: '', meetingLink: '' };

  constructor(private http: HttpClient, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  loadMeetings() {
    this.http.get<any[]>(`${environment.apiUrl}/meetings`).subscribe({
      next: (res) => (this.meetings = res),
      error: () => this.toastr.error('Failed to load meetings')
    });
  }

  createMeeting() {
    this.http.post(`${environment.apiUrl}/meetings`, this.newMeeting).subscribe({
      next: () => {
        this.toastr.success('Meeting created');
        this.newMeeting = { title: '', description: '', scheduledAt: '', meetingLink: '' };
        this.loadMeetings();
      },
      error: () => this.toastr.error('Failed to create meeting')
    });
  }

  deleteMeeting(id: number) {
    if (!confirm('Are you sure you want to delete this meeting?')) return;

    this.http.delete(`${environment.apiUrl}/meetings/${id}`).subscribe({
      next: () => {
        this.toastr.info('Meeting deleted');
        this.loadMeetings();
      },
      error: () => this.toastr.error('Failed to delete meeting')
    });
  }
}
