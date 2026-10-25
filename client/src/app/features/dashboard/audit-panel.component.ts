import { JsonPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FindingsService } from '../../core/services/findings.service';

@Component({
  selector: 'app-audit-panel',
  standalone: true,
  imports: [JsonPipe],
  templateUrl: './audit-panel.component.html',
})
export class AuditPanelComponent {
  protected readonly logs = signal<unknown[]>([]);
  protected readonly error = signal<string | null>(null);

  constructor() {
    inject(FindingsService).auditLogs().subscribe({
      next: (l) => this.logs.set(l),
      error: () => this.error.set('you are not allowed to see the audit log'),
    });
  }
}
