import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ContactMessage, ContactService } from '../../../core/services/contact.service';

@Component({
  selector: 'app-admin-contact-messages',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './admin-contact-messages.html',
})
export class AdminContactMessages implements OnInit {
  readonly messages = signal<ContactMessage[]>([]);
  readonly loading = signal(true);

  constructor(private readonly contactService: ContactService) {}

  ngOnInit(): void {
    this.contactService.listForAdmin().subscribe({
      next: (messages) => {
        this.messages.set(messages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
