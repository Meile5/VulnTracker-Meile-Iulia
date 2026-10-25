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
  protected readonly reference = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);

  protected send() {
    this.api
      .submitPublic({
        title: this.title,
        description: this.description,
        affectedAsset: this.asset,
        contactEmail: this.email.trim(),
      })
      .subscribe({
        next: (r) => this.reference.set(r.reference),
        error: (e) =>
          this.error.set(
            e.status === 429 ? 'too many reports, wait a minute and try again'
            : e.status === 400 ? 'please check the fields, the email must be valid'
            : 'could not send the report, try again',
          ),
      });
  }
}
