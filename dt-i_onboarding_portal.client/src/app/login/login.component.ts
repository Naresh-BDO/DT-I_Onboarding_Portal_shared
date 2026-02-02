import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; // <-- ADDED

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username = '';
  password = '';
  userInfo: any;
  isSubmitting = false;

  error = '';
  success = '';

  // Inject Router here
  constructor(
    private auth: AuthService,
    private http: HttpClient,
    private router: Router // <-- ADDED
  ) { }

  onLogin() {
    if (this.isSubmitting) return;
    this.error = '';
    this.success = '';
    this.isSubmitting = true;

    this.auth.login(this.username, this.password)
      .subscribe({
        next: (res: any) => {
          console.log('Login response:', res);
          // FIX: Handle both 'token' and 'Token'
          const jwt = res.token || res.Token;
          if (jwt) {
            this.auth.saveToken(jwt);
            this.success = 'Login successful!';
            this.isSubmitting = false;

            if (this.auth.isAdmin()) {
              this.router.navigate(['/new-joiner']);
            } else {
              this.router.navigate(['/']);
            }
          } else {
            this.error = 'No token received from server.';
            this.isSubmitting = false;
          }
        },
        error: (err) => {
          console.error('Login error:', err);
          const status = err?.status;
          if (status === 401 || status === 400) {
            this.error = 'Invalid username or password.';
          } else if (status === 0) {
            this.error = 'Cannot reach server. Check if https://localhost:7107 is running and CORS is enabled.';
          } else {
            this.error = `Login failed (status ${status}).`;
          }
          this.isSubmitting = false;
        }
      });
  }

  whoAmI() {
    this.error = '';
    this.success = '';
    this.http.get('https://localhost:7107/api/Auth/whoami').subscribe({
      next: (res: any) => {
        this.userInfo = res;
        this.success = 'Fetched current user info.';
      },
      error: () => {
        this.error = 'Not authorized or server unreachable.';
      }
    });
  }
}