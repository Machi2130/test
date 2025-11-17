import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginService } from './login.service';
import { LoginRequest } from '../Models/login-request';
import { LoginResponse } from '../Models/login-response';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private tokenKey = 'auth-token';
  private userKey = 'auth-user';

  constructor(private loginService: LoginService) { }

  // ✅ Login using LoginService (proxy handled internally)
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.loginService.login(request).pipe(
      tap(res => {
        if (res && res.token) {
          // Store JWT token
          localStorage.setItem(this.tokenKey, res.token);

          // Store user details (id, username, roles, permissions)
          localStorage.setItem(this.userKey, JSON.stringify(res));
        }
      })
    );
  }

  // ✅ Logout - clear local storage
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  // ✅ Get stored JWT token
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // ✅ Get logged-in user info
  getUser(): LoginResponse | null {
    const user = localStorage.getItem(this.userKey);
    return user ? (JSON.parse(user) as LoginResponse) : null;
  }

  // ✅ Check if user is logged in
  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  // ✅ Check permission
  hasPermission(permission: string): boolean {
    const user = this.getUser();
    return user?.permissions?.includes(permission) ?? false;
  }
}
