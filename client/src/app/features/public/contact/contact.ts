import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { PhoneMaskDirective } from '../../../shared/directives/phone-mask.directive';

/** Atelier's WhatsApp number in E.164 (no symbols), used to build the wa.me deep link. */
const WHATSAPP_NUMBER = '5511913130481';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [ReactiveFormsModule, PhoneMaskDirective],
  templateUrl: './contact.html',
})
export class Contact implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly pieceTypes = ['Fralda de Ombro', 'Fralda de Boca', 'Kit Ombro e Boca', 'Outro'];
  readonly sizes = ['Padrão', 'Grande', 'Sob medida'];

  readonly form = this.fb.nonNullable.group({
    isCustomOrder: [false],
    customerName: ['', Validators.required],
    customerEmail: ['', Validators.email],
    customerPhone: ['', Validators.required],
    tipoPeca: [this.pieceTypes[0]],
    tamanho: [this.sizes[0]],
    tecido: [''],
    cor: [''],
    nomeBordado: [''],
    message: ['', Validators.required],
  });

  ngOnInit(): void {
    const user = this.auth.currentUser();
    if (user) {
      this.form.patchValue({ customerName: user.name, customerEmail: user.email });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    window.open(this.buildWhatsAppUrl(), '_blank', 'noopener');
  }

  private buildWhatsAppUrl(): string {
    const v = this.form.getRawValue();
    const lines = [`Olá! Meu nome é ${v.customerName}.`];

    if (v.isCustomOrder) {
      lines.push('', 'Gostaria de fazer uma encomenda personalizada:');
      lines.push(`- Tipo de peça: ${v.tipoPeca}`);
      lines.push(`- Tamanho: ${v.tamanho}`);
      if (v.tecido) lines.push(`- Tecido desejado: ${v.tecido}`);
      if (v.cor) lines.push(`- Cor: ${v.cor}`);
      if (v.nomeBordado) lines.push(`- Nome para bordar: ${v.nomeBordado}`);
    }

    lines.push('', v.message);

    if (v.customerEmail) lines.push('', `E-mail: ${v.customerEmail}`);
    if (v.customerPhone) lines.push(`Telefone: ${v.customerPhone}`);

    const text = encodeURIComponent(lines.join('\n'));
    return `https://wa.me/${WHATSAPP_NUMBER}?text=${text}`;
  }
}
