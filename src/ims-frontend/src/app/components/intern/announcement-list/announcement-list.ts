import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnnouncementService, Announcement } from '../../../services/announcement.service';

@Component({
  selector: 'app-announcement-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './announcement-list.html',
  styleUrls: ['./announcement-list.scss']
})
export class AnnouncementListComponent implements OnInit {
  announcements: Announcement[] = [];
  loading = true;

  constructor(private announcementService: AnnouncementService) {}

  ngOnInit(): void {
    this.announcementService.getMyAnnouncements().subscribe({
      next: (res) => {
        this.announcements = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load announcements', err);
        this.loading = false;
      }
    });
  }
}
