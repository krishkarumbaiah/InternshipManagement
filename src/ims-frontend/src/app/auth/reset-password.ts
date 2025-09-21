import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth';
import { ActivatedRoute, Router ,RouterModule} from '@angular/router';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.html',
})
export class ResetPasswordComponent {
  form: FormGroup;
  message: string = '';
  loading = false;
  email: string = '';
  token: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
    });

    this.email = decodeURIComponent(this.route.snapshot.queryParamMap.get('email') || '');
    this.token = decodeURIComponent(this.route.snapshot.queryParamMap.get('token') || '');

  }

  submit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.authService.resetPassword(this.email, this.token, this.form.value.newPassword).subscribe({
      next: () => {
        this.message = 'Password has been reset successfully.';
        this.loading = false;

        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        if (typeof err.error === 'string') {
          this.message = 'Error: ' + err.error;
        } else if (err.error?.message) {
          this.message = 'Error: ' + err.error.message;
        } else if (err.error?.errors) {
          // ASP.NET Identity usually returns dictionary of errors
          const firstError = Object.values(err.error.errors).flat()[0];
          this.message = 'Error: ' + firstError;
        } else {
          this.message = 'Error: ' + JSON.stringify(err.error);
        }
        this.loading = false;
      },
    });
  }
}
