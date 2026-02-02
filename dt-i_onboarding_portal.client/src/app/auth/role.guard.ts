import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const required: string[] = route.data['roles'] ?? [];
    if (required.length === 0) return true;
    const roles = this.auth.getRoles();
    const ok = required.some(r => roles.includes(r));
    if (!ok) {
      // optionally redirect to unauthorized page
      this.router.navigate(['/']);
    }
    return ok;
  }
}
