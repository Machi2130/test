import { Component } from '@angular/core';
import { LoginRequest } from '../Models/login-request';
import { AuthService } from '../Services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  model: LoginRequest = { username: '', password: '', device: '' };
  errorMessage: string = '';

  constructor(private auth: AuthService, private router: Router) { }

  onSubmit() {
    this.auth.login(this.model).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Login failed. Please try again.';
      }
    });
  }
}
