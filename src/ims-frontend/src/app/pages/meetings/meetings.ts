import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-meetings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './meetings.html',
  styleUrls: ['./meetings.scss']
})
export class MeetingsComponent implements OnInit {
  meetings: any[] = [];
  apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  
  loadMeetings() {
    this.http.get<any[]>(`${this.apiUrl}/meetings/upcoming/mine`).subscribe({
      next: (res) => (this.meetings = res),
      error: (err) => {
        console.error('Failed to load meetings', err);
        this.toastr.error(err.error?.message || 'Failed to load meetings');
      }
    });
  }

  isExpired(date: string | Date): boolean {
    return new Date(date) < new Date();
  }
}
