import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { MainLayoutComponent } from './layout/main-layout/main-layout';
import { BatchesComponent } from './pages/batches/batches';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'batches', component: BatchesComponent }
    ]
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];
