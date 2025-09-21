import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseService } from '../../../services/course';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './courses.html',
  styleUrls: ['./courses.scss']
})
export class CoursesComponent implements OnInit {
  courses: any[] = [];
  myCourseIds = new Set<number>();
  message = '';
  loading = false;

  constructor(private courseSvc: CourseService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.courseSvc.getCourses().subscribe((c: any[]) => {
      this.courses = c;
      this.courseSvc.getMyEnrollments().subscribe((list: any[]) => {
        this.myCourseIds = new Set(list.map((x: any) => x.id));
        this.loading = false;
      });
    });
  }

  enroll(courseId: number) {
    this.courseSvc.enroll(courseId).subscribe({
      next: () => {
        this.message = 'Enrolled successfully!';
        this.load();
      },
      error: (err: any) => this.message = err.error || 'Error enrolling'
    });
  }

  unenroll(courseId: number) {
    this.courseSvc.unenroll(courseId).subscribe({
      next: () => {
        this.message = 'Unenrolled successfully!';
        this.load();
      },
      error: (err: any) => this.message = err.error || 'Error unenrolling'
    });
  }
}
