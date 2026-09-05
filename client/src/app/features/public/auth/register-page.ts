import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.html',
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth
      .register({ name: value.name, email: value.email, password: value.password, phone: value.phone || null })
      .subscribe({
        next: () => this.router.navigate(['/minha-conta']),
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(err?.error?.detail ?? 'Não foi possível criar sua conta.');
        },
      });
  }
}
