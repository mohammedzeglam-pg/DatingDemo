import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  members: Member[] = [];
  constructor(private readonly http: HttpClient) {}

  getMembers() {
    if (this.members.length > 0) return of(this.members);
    return this.http.get<Member[]>('/api/users').pipe(
      map((members) => {
        this.members = members;
        return members;
      })
    );
  }
  getMember(username: string) {
    const member = this.members.find((memb) => memb.username === username);
    if (member !== undefined) return of(member);
    return this.http.get<Member>('/api/users/' + username);
  }
  updateMember(member: Member) {
    return this.http.put('/api/users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }
}
