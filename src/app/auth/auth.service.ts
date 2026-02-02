import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { LoginRequest } from '../models/login-request';
import { LoginResponse } from '../models/login-response';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private tokenKey = 'auth_token';

    constructor(private http: HttpClient) { }

    login(req: LoginRequest) {
        return this.http.post<LoginResponse | any>(`${environment.apiUrl}/api/auth/login`, req);
    }

    setToken(token: string) {
        localStorage.setItem(this.tokenKey, token);
    }

    getToken(): string | null {
        return localStorage.getItem(this.tokenKey);
    }

    logout() {
        localStorage.removeItem(this.tokenKey);
    }

    isAuthenticated(): boolean {
        const t = this.getToken();
        if (!t) return false;
        const payload = decodeJwtPayload(t);
        if (!payload) return false;
        const exp = payload.exp;
        if (!exp) return true;
        return Math.floor(Date.now() / 1000) < exp;
    }

    getRoles(): string[] {
        const t = this.getToken();
        if (!t) return [];
        const payload = decodeJwtPayload(t);
        if (!payload) return [];
        // roles may be in claim types: role, roles, http://schemas.microsoft.com/ws/2008/06/identity/claims/role
        const r = payload.role ?? payload.roles ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        if (!r) return [];
        if (Array.isArray(r)) return r;
        return [r];
    }
}

// minimal JWT payload decode (no signature validation)
function decodeJwtPayload(token: string): any | null {
    try {
        const parts = token.split('.');
        if (parts.length < 2) return null;
        const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
        const json = decodeURIComponent(atob(payload).split('').map(c =>
            '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        ).join(''));
        return JSON.parse(json);
    } catch {
        return null;
    }
}