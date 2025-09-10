import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],  // ✅ added RouterModule
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  model = { userName: '', password: '' };

  constructor(
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  login() {
    this.auth.login(this.model).subscribe({
      next: (res: any) => {
        this.auth.saveToken(res.token);
        this.toastr.success('Login successful!', 'Success');
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.toastr.error('Invalid username or password', 'Login Failed');
      }
    });
  }
}
