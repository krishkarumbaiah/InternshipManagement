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
  submitting = false;
  selectedFile?: File;
  imagePreviewUrl?: string | ArrayBuffer | null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      username: ['', Validators.required],   // mapped to userName in payload
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      otp: ['']   // OTP will be required only after sending
    });

    // initially OTP not required until sent
    this.registerForm.get('otp')?.clearValidators();
    this.registerForm.get('otp')?.updateValueAndValidity();
  }

  // handle file input changes
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.selectedFile = undefined;
      this.imagePreviewUrl = undefined;
      return;
    }

    const file = input.files[0];

    // client-side validations
    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowed.includes(file.type)) {
      this.toastr.error('Only JPG, PNG or WEBP files are allowed.');
      input.value = '';
      return;
    }
    const maxBytes = 2 * 1024 * 1024; // 2 MB
    if (file.size > maxBytes) {
      this.toastr.error('Profile photo size should be 2 MB or less.');
      input.value = '';
      return;
    }

    this.selectedFile = file;

    // preview
    const reader = new FileReader();
    reader.onload = () => {
      this.imagePreviewUrl = reader.result;
    };
    reader.readAsDataURL(file);
  }

  // Send OTP to email
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
        // make OTP field required after sending
        this.registerForm.get('otp')?.setValidators([Validators.required]);
        this.registerForm.get('otp')?.updateValueAndValidity();
      },
      error: () => this.toastr.error('❌ Failed to send OTP')
    });
  }

  // Submit registration (sends FormData if file selected)
  onSubmit() {
    if (this.registerForm.invalid) {
      this.toastr.warning('Please fill all required fields correctly');
      this.registerForm.markAllAsTouched();
      return;
    }

    // If a file was selected -> send FormData
    let payload: any;
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('UserName', this.registerForm.get('username')?.value);
      formData.append('Email', this.registerForm.get('email')?.value);
      formData.append('Password', this.registerForm.get('password')?.value);
      formData.append('FullName', this.registerForm.get('fullName')?.value ?? '');
      formData.append('Otp', this.registerForm.get('otp')?.value ?? '');
      formData.append('ProfilePhoto', this.selectedFile, this.selectedFile.name);
      payload = formData;
    } else {
      // plain JSON fallback
      payload = {
        userName: this.registerForm.get('username')?.value,
        email: this.registerForm.get('email')?.value,
        password: this.registerForm.get('password')?.value,
        fullName: this.registerForm.get('fullName')?.value ?? '',
        otp: this.registerForm.get('otp')?.value ?? ''
      };
    }

    this.submitting = true;
    this.auth.register(payload).subscribe({
      next: () => {
        this.toastr.success('Registration successful 🎉');
        this.submitting = false;
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.submitting = false;
        console.error('Register error', err);

        // parse common server error shapes:
        const body = err?.error;
        let message = 'Failed to register. Try again.';

        if (!body) {
          message = err?.message ?? message;
        } else if (typeof body === 'string') {
          message = body;
        } else if (body?.message) {
          message = body.message;
        } else if (body?.errors) {
          if (Array.isArray(body.errors)) {
            message = body.errors.join('; ');
          } else if (typeof body.errors === 'object') {
            const all: string[] = [];
            for (const k in body.errors) {
              if (Array.isArray(body.errors[k])) all.push(...body.errors[k]);
              else all.push(body.errors[k]);
            }
            if (all.length) message = all.join('; ');
          }
        } else if (Array.isArray(body)) {
          message = body.map((x: any) => x.description || x).join('; ');
        }

        this.toastr.error(message, 'Registration failed');
      }
    });
  }
}
