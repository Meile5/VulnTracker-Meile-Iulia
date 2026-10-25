import { Component, computed, inject, signal } from '@angular/core';
import { FindingsService } from '../../core/services/findings.service';
import { Summary, statusLabel } from '../../core/models/finding.models';

@Component({
  selector: 'app-overview-panel',
  standalone: true,
  templateUrl: './overview-panel.component.html',
})
export class OverviewPanelComponent {
  protected readonly summary = signal<Summary | null>(null);
  protected readonly error = signal<string | null>(null);

  protected readonly severityRows = computed(() => this.rows(this.summary()?.openBySeverity));
  protected readonly statusRows = computed(() => this.rows(this.summary()?.byStatus));

  constructor() {
    inject(FindingsService).summary().subscribe({
      next: (s) => this.summary.set(s),
      error: () => this.error.set('could not load the summary'),
    });
  }

  // turns { High: 3, Low: 1 } into rows with a bar width in percent
  private rows(r?: Record<string, number>) {
    const entries = Object.entries(r ?? {});
    const max = Math.max(1, ...entries.map(([, n]) => n));
    return entries.map(([name, count]) => ({ label: statusLabel(name), count, pct: (count / max) * 100 }));
  }
}
