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
      role: ['Intern', Validators.required] // Default role
    });
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.toastr.warning('Please fill all required fields correctly');
      return;
    }

    this.auth.register(this.registerForm.value).subscribe({
      next: () => {
        this.toastr.success('Registration successful 🎉');
        this.router.navigate(['/login']);
      },
      error: () => {
        this.toastr.error('Failed to register. Try again.');
      }
    });
  }
}
