import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/user';
@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();
  constructor(private http: HttpClient) {}

  login(model: any) {
    return this.http.post<User>('/api/account/login', model).pipe(
      map((response: User) => {
        this.setCurrentUser(response);
      })
    );
  }
  register(model: any) {
    return this.http.post<User>('/api/account/register', model).pipe(
      map((response: User) => {
        this.setCurrentUser(response);
      })
    );
  }
  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }
  logout(): void {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
