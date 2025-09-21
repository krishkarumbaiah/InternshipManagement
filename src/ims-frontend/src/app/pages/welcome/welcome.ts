import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-welcome',
  standalone: true,   
  imports: [],
  templateUrl: './welcome.html',
  styleUrls: ['./welcome.scss']
})
export class Welcome {
  roles: string[] = [];

  constructor(private router: Router) {
    this.roles = JSON.parse(localStorage.getItem('roles') || '[]'); 
  }

  goToDashboard() {
    if (this.roles.includes('Admin')) {
      this.router.navigate(['/dashboard']); 
    } else if (this.roles.includes('Intern')) {
      this.router.navigate(['/dashboard']); 
    } else {
      this.router.navigate(['/login']); 
    }
  }
}
