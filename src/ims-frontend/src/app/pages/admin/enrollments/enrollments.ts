import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseService } from '../../../services/course';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-admin-enrollments',
  standalone: true,
  imports: [CommonModule, TableModule, CardModule, ProgressSpinnerModule, ButtonModule, ToastModule],
  templateUrl:'./enrollments.html',
  styleUrls: ['./enrollments.scss']
})
export class AdminEnrollmentsComponent implements OnInit {
  enrollments: any[] = [];
  loading = false;

  constructor(private courseSvc: CourseService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.courseSvc.getAllEnrollments().subscribe((list: any[]) => {
      this.enrollments = list;
      this.loading = false;
    });
  }
}
