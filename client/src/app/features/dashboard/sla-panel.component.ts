import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FindingsService } from '../../core/services/findings.service';
import { SEVERITIES } from '../../core/models/finding.models';

@Component({
  selector: 'app-sla-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './sla-panel.component.html',
})
export class SlaPanelComponent {
  private readonly api = inject(FindingsService);
  protected readonly severities = SEVERITIES;
  protected severity = 'High';
  protected days = 7;
  protected readonly msg = signal<string | null>(null);

  protected save() {
    this.api.setSla(this.severity, this.days).subscribe({
      next: () => this.msg.set('saved ✨ (the API is still a stub, it does not store it yet)'),
      error: () => this.msg.set('could not save, only PMs can do this'),
    });
  }
}
