import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ContactMessage {
  id: string;
  name: string;
  email: string;
  message: string;
  createdAt: string;
}

export interface SubmitContactRequest {
  name: string;
  email: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ContactService {
  constructor(private readonly http: HttpClient) {}

  submit(request: SubmitContactRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/contact`, request);
  }

  listForAdmin(): Observable<ContactMessage[]> {
    return this.http.get<ContactMessage[]>(`${environment.apiUrl}/admin/contact-messages`);
  }
}
