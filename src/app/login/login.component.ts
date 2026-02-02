import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { LoginRequest } from '../models/login-request';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';
    const req: LoginRequest = { username: this.username, password: this.password };
    this.auth.login(req).subscribe({
      next: (res: any) => {
        // support token property name both 'token' and 'Token'
        const token = res.token ?? res.Token ?? res.TokenString ?? res.tokenString;
        if (!token) {
          this.error = 'No token in response';
          return;
        }
        this.auth.setToken(token);
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Login failed';
      }
    });
  }
}