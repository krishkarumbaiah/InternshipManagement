import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { MainLayoutComponent } from './layout/main-layout/main-layout';
import { BatchesComponent } from './pages/batches/batches';
import { AssignmentsComponent } from './pages/assignments/assignments';
import { authGuard } from './guards/auth-guard';
import { QnaComponent } from './pages/qna/qna';
import { QnaAdminComponent } from './pages/qna-admin/qna-admin';
import { AttendanceAdminComponent } from './pages/attendance-admin/attendance-admin';
import { AttendanceInternComponent } from './pages/attendance-intern/attendance-intern';
import { MeetingsAdminComponent } from './pages/meetings-admin/meetings-admin';
import { MeetingsComponent } from './pages/meetings/meetings';
import { NotificationsComponent } from './pages/notifications/notifications';
import { ForgotPasswordComponent } from './auth/forgot-password';
import { ResetPasswordComponent } from './auth/reset-password';
import { CoursesComponent } from './pages/courses/courses/courses';
import { AdminEnrollmentsComponent } from './pages/admin/enrollments/enrollments';
import { UploadDocumentsComponent } from './intern/upload-documents/upload-documents';
import { ManageDocumentsComponent } from './intern/manage-documents/manage-documents';
import { Welcome } from './pages/welcome/welcome';  

export const routes: Routes = [
  // Public routes
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },

  // Protected (requires auth)
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      
      { path: '', redirectTo: 'welcome', pathMatch: 'full' },

      // Welcome page
      { path: 'welcome', component: Welcome },

      // Common
      { path: 'dashboard', component: DashboardComponent },

      // Intern routes
      { path: 'qna', component: QnaComponent, data: { roles: ['Intern'] } },
      { path: 'attendance', component: AttendanceInternComponent, data: { roles: ['Intern'] } },
      { path: 'meetings', component: MeetingsComponent, data: { roles: ['Intern'] } },
      { path: 'notifications', component: NotificationsComponent, data: { roles: ['Intern'] } },
      { path: 'courses', component: CoursesComponent, data: { roles: ['Intern'] } },
      { path: 'upload-documents', component: UploadDocumentsComponent, data: { roles: ['Intern'] } },

      // Admin routes
      { path: 'qna-admin', component: QnaAdminComponent, data: { roles: ['Admin'] } },
      { path: 'attendance-admin', component: AttendanceAdminComponent, data: { roles: ['Admin'] } },
      { path: 'batches', component: BatchesComponent, data: { roles: ['Admin'] } },
      { path: 'assignments', component: AssignmentsComponent, data: { roles: ['Admin'] } },
      { path: 'meetings-admin', component: MeetingsAdminComponent, data: { roles: ['Admin'] } },
      { path: 'admin/enrollments', component: AdminEnrollmentsComponent, data: { roles: ['Admin'] } },
      { path: 'manage-documents', component: ManageDocumentsComponent, data: { roles: ['Admin'] } }
    ]
  },

  // Fallback
  { path: '**', redirectTo: 'login' }
];
