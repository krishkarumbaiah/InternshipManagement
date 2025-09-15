// src/app/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');
  const roles = JSON.parse(localStorage.getItem('roles') || '[]');

  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  // ✅ Role-based check
  const requiredRoles = route.data?.['roles'] as string[] | undefined;
  if (requiredRoles && !roles.some((r: string) => requiredRoles.includes(r))) {
    alert('Access denied: You do not have permission to view this page.');
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};
