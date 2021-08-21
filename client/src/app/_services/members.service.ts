import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  constructor(private readonly http: HttpClient) {}

  getMembers() {
    return this.http.get<Member[]>('/api/users');
  }
  getMember(username: string) {
    return this.http.get<Member>('/api/users/' + username);
  }
}
