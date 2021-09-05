import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';
@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();
  constructor(private http: HttpClient, private presence: PresenceService) {}

  login(model: any) {
    return this.http.post<User>('/api/account/login', model).pipe(
      map((response: User) => {
        this.setCurrentUser(response);
        this.presence.createHubConnection(response);
      })
    );
  }
  register(model: any) {
    return this.http.post<User>('/api/account/register', model).pipe(
      map((response: User) => {
        this.setCurrentUser(response);
        this.presence.createHubConnection(response);
      })
    );
  }
  setCurrentUser(user: User) {
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles) ? (user.roles = roles) : user.roles.push(roles);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }
  logout(): void {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presence.stopHubConnection();
  }
  getDecodedToken(token: string) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
