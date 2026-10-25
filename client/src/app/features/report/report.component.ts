import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FindingsService } from '../../core/services/findings.service';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './report.component.html',
})
export class ReportComponent {
  private readonly api = inject(FindingsService);
  protected title = '';
  protected description = '';
  protected asset = '';
  protected email = '';
  protected readonly sent = signal(false);
  protected readonly error = signal<string | null>(null);

  protected send() {
    this.api
      .submitPublic({
        title: this.title,
        description: this.description,
        affectedAsset: this.asset,
        contactEmail: this.email.trim() || null,
      })
      .subscribe({
        next: () => this.sent.set(true),
        error: () => this.error.set('could not send the report, try again'),
      });
  }
}
