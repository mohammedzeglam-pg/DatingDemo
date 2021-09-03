import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeader } from './PaginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  constructor(private http: HttpClient) {}

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeader(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>('/api/messages', params, this.http);
  }
  getMessageThread(username: string) {
    return this.http.get<Message[]>('/api/messages/thread/' + username);
  }
  sendMessage(username: string, content: string) {
    return this.http.post<Message>('/api/messages', {
      recipientUsername: username,
      content,
    });
  }
  deleteMessage(id: number) {
    return this.http.delete('/api/messages/' + id);
  }
}
