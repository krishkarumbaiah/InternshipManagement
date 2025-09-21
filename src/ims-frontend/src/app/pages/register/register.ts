import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.scss']
})
export class RegisterComponent {
  registerForm: FormGroup;
  otpSent = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Intern', Validators.required],
      otp: ['', Validators.required]   
    });
  }

  //  Send OTP
  sendOtp() {
    const email = this.registerForm.get('email')?.value;
    if (!email) {
      this.toastr.warning('Please enter a valid email first');
      return;
    }

    this.auth.sendOtp(email).subscribe({
      next: () => {
        this.toastr.success('✅ OTP sent to your email 📧', 'Success');
        this.otpSent = true;
        this.registerForm.get('otp')?.setValidators([Validators.required]);
        this.registerForm.get('otp')?.updateValueAndValidity();
      },
      error: () => this.toastr.error('❌ Failed to send OTP')
    });
  }

  //  Submit with OTP verification
  onSubmit() {
    if (this.registerForm.invalid) {
      this.toastr.warning('Please fill all required fields correctly');
      return;
    }

    if (!this.registerForm.value.otp) {
      this.toastr.warning('Please enter the OTP sent to your email');
      return;
    }

    this.auth.register(this.registerForm.value).subscribe({
      next: () => {
        this.toastr.success('Registration successful 🎉');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Failed to register. Try again.');
      }
    });
  }
}
