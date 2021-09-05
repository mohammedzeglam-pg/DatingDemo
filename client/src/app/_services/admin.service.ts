import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  constructor(private http: HttpClient) {}
  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>('/api/admin/users-with-roles');
  }
  updateUserRoles(username: string, roles: string[]) {
    //TODO: fix it
    return this.http.post(
      '/api/admin/edit-roles/' + username + '?roles=' + roles,
      {}
    );
  }
}
