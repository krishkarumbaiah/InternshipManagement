import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.html',
})
export class ForgotPasswordComponent {
  form: FormGroup;
  message: string = '';
  loading = false;

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  submit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.authService.forgotPassword(this.form.value.email).subscribe({
      next: () => {
        this.message = 'Password reset link has been sent to your email.';
        this.loading = false;
      },
      error: (err) => {
        if (typeof err.error === 'string') {
          this.message = 'Error: ' + err.error;
        } else if (err.error?.message) {
          this.message = 'Error: ' + err.error.message;
        } else {
          this.message = 'Error: Something went wrong';
        }
        this.loading = false;
      },
    });
  }
}
