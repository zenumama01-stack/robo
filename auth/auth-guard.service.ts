import { Router, CanActivate } from '@angular/router';
import { Observable, map } from 'rxjs';
export class AuthGuardService implements CanActivate {
  constructor(private authBase: MJAuthBase, public router: Router) {}
  canActivate(): Observable<boolean> {
    // v3.0 API - use observable instead of property
    return this.authBase.isAuthenticated().pipe(
      map(isAuthenticated => {
