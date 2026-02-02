import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";


@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'https://localhost:7107/api/Auth'; // Base URL only
  private tokenKey = 'jwt_token';
  private loggedIn = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) {
    this.loggedIn.next(!!this.getToken());
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { username, password }); // Correct endpoint
  }

  saveToken(token: string) {
    localStorage.setItem(this.tokenKey, token);
    this.loggedIn.next(true);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    this.loggedIn.next(false);
  }

  isLoggedIn(): Observable<boolean> {
    return this.loggedIn.asObservable();
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];
    const payload = JSON.parse(atob(token.split('.')[1]));
    let roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || [];
    if (typeof roles === 'string') roles = [roles];
    return roles;
  }
//newly added.
  isAdmin(): boolean {
    return this.getRoles().includes('Admin');
  }

}
