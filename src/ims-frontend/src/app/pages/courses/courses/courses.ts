import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseService } from '../../../services/course';
import { ToastrService } from 'ngx-toastr';


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

  constructor(private courseSvc: CourseService,
    private toastr: ToastrService
  ) {}

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
        this.toastr.success('Enrolled successfully! 🎉', 'Success');
        this.load();
      },
      error: (err: any) => this.toastr.error(err.error || 'Error enrolling', 'Error')
    });
  }

  unenroll(courseId: number) {
    this.courseSvc.unenroll(courseId).subscribe({
      next: () => {
        this.toastr.info('Unenrolled successfully! 👋', 'Info');
        this.load();
      },
      error: (err: any) => this.toastr.error(err.error || 'Error unenrolling', 'Error')
    });
  }
}
