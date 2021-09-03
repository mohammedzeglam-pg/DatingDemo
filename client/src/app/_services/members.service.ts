import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { Member } from '../_models/member';
import { User } from '../_models/user';
import { UserParams } from '../_models/user-params';
import { AccountService } from './account.service';
import { getPaginatedResult, getPaginationHeader } from './PaginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  private userParams: UserParams;
  constructor(
    private readonly http: HttpClient,
    private readonly accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe((user) => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }
  getUserParams() {
    return this.userParams;
  }
  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }
  getMembers(userParams: UserParams) {
    let key = Object.values(userParams).join('-');
    let response = this.memberCache.get(key);
    if (response) {
      return of(response);
    }
    let params = getPaginationHeader(
      userParams.pageNumber,
      userParams.pageSize
    );
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    return getPaginatedResult<Member[]>('/api/users', params, this.http).pipe(
      map((response) => {
        this.memberCache.set(key, response);
        return response;
      })
    );
  }
  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.username === username);
    if (member) {
      return of(member);
    }
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
  setMainPhoto(photoId: number) {
    return this.http.put('/api/users/set-main-photo/' + photoId, {});
  }
  deletePhoto(photoId: number) {
    return this.http.delete('/api/users/delete-photo/' + photoId);
  }

  addLike(username: string) {
    return this.http.post('/api/likes/' + username, {});
  }
  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = getPaginationHeader(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    return getPaginatedResult<Partial<Member[]>>(
      '/api/likes',
      params,
      this.http
    );
  }
}
