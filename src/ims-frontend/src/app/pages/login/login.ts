import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  login() {
    if (this.loginForm.invalid) {
      this.toastr.warning('Please fill in all fields');
      return;
    }

    this.auth.login(this.loginForm.value).subscribe({
      next: (res: any) => {
        this.auth.saveAuthData(res.token, res.roles);
        this.toastr.success('Login successful!', 'Success');
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.toastr.error('Invalid username or password', 'Login Failed');
      }
    });
  }
}
