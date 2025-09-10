import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr'
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule], 
  templateUrl: './register.html',
  styleUrls: ['./register.scss'],
})
export class RegisterComponent {
  model = {
    userName: '',
    email: '',
    password: '',
    fullName: '',
    role: 'Intern', // default role
  };

  constructor(private auth: AuthService, private router: Router, private toastr: ToastrService) {}

register() {
  this.auth.register(this.model).subscribe({
    next: () => {
      this.toastr.success('Registration successful!', 'Success');
      this.router.navigate(['/login']);
    },
    error: (err) => {
      console.error(err);
      let message = 'Registration failed';
      if (err.error) {
        if (typeof err.error === 'string') {
          message = err.error;
        } else if (Array.isArray(err.error)) {
          // Identity returns errors as an array
          message = err.error.map((e: any) => e.description).join(', ');
        } else if (err.error.message) {
          message = err.error.message;
        }
      }
      this.toastr.error(message, 'Error');
    }
  });
}
}
