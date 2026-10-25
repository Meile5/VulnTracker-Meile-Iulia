import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { AuthStore } from '../../core/stores/auth.store';
import { FindingsService } from '../../core/services/findings.service';
import { Finding, SEVERITIES, STATUSES, statusLabel } from '../../core/models/finding.models';

type Mode = 'create' | 'edit' | 'assign';
type TagSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary';

const blankFilters = () => ({ text: '', asset: '', status: '', severity: '', mine: false });
type Filters = ReturnType<typeof blankFilters>;

const blankForm = () => ({
  title: '', description: '', affectedAsset: '', severity: 'Medium',
  cvssScore: null as number | null, status: 'New', assigneeId: '', dueDate: '',
});

@Component({
  selector: 'app-findings-panel',
  standalone: true,
  imports: [FormsModule, TableModule, TagModule],
  templateUrl: './findings-panel.component.html',
})
export class FindingsPanelComponent {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(FindingsService);

  protected readonly severities = SEVERITIES;
  protected readonly statuses = STATUSES;
  protected readonly label = statusLabel;

  protected readonly findings = signal<Finding[]>([]);
  protected readonly filters = signal<Filters>(blankFilters());
  protected readonly loading = signal(false);
  protected readonly message = signal<string | null>(null);
  protected readonly mode = signal<Mode | null>(null);
  protected readonly selected = signal<Finding | null>(null);
  protected form = blankForm();
  private timer?: ReturnType<typeof setTimeout>;

  constructor() {
    this.load();
  }

  // same rules as the API policies, only used to hide buttons (the API still checks)
  protected canCreate() { return this.auth.hasRole('analyst') || this.auth.hasRole('pm'); }
  protected canAssign() { return this.canCreate(); }
  protected canEdit() { return this.canCreate() || this.auth.hasRole('developer'); }

  protected isOverdue(f: Finding) {
    return !!f.dueDate && new Date(f.dueDate) < new Date() && !['Remediated', 'AcceptedRisk'].includes(f.status);
  }

  protected tagSeverity(s: string): TagSeverity {
    return ({ Critical: 'danger', High: 'warn', Medium: 'info', Low: 'success' } as Record<string, TagSeverity>)[s] ?? 'secondary';
  }

  protected setFilter<K extends keyof Filters>(key: K, value: Filters[K]) {
    this.filters.update((f) => ({ ...f, [key]: value }));
    clearTimeout(this.timer);
    this.timer = setTimeout(() => this.load(), 300); // wait a bit while typing
  }

  protected load(): void {
    const f = this.filters();
    this.loading.set(true);
    this.api
      .list({
        text: f.text, asset: f.asset, status: f.status, severity: f.severity,
        assigneeId: f.mine ? this.auth.user()?.id : undefined,
      })
      .subscribe({
        next: (r) => { this.findings.set(r); this.loading.set(false); },
        error: (e) => this.fail(e),
      });
  }

  protected openCreate() { this.form = blankForm(); this.message.set(null); this.mode.set('create'); }
  protected openEdit(f: Finding) {
    this.selected.set(f);
    this.form = { ...blankForm(), title: f.title, description: f.description, status: f.status, severity: f.severity };
    this.message.set(null);
    this.mode.set('edit');
  }
  protected openAssign(f: Finding) {
    this.selected.set(f);
    this.form = { ...blankForm(), assigneeId: f.assigneeId ?? '', dueDate: f.dueDate?.slice(0, 10) ?? '' };
    this.message.set(null);
    this.mode.set('assign');
  }
  protected assignToMe() { this.form.assigneeId = this.auth.user()?.id ?? ''; }
  protected close() { this.mode.set(null); }

  protected save(): void {
    const f = this.selected();
    const x = this.form;
    // date input gives "2026-11-01", the API wants a full timestamp (end of that day)
    const dueIso = x.dueDate ? new Date(`${x.dueDate}T23:59:59`).toISOString() : null;

    const req =
      this.mode() === 'create'
        ? this.api.create({ title: x.title, description: x.description, affectedAsset: x.affectedAsset, severity: x.severity, cvssScore: x.cvssScore })
        : this.mode() === 'edit' && f
          ? this.api.update(f.id, { title: x.title, description: x.description, status: x.status, severity: x.severity })
          : this.mode() === 'assign' && f
            ? this.api.assign(f.id, x.assigneeId.trim(), dueIso)
            : null;

    req?.subscribe({
      next: () => { this.close(); this.load(); },
      error: (e) => this.fail(e),
    });
  }

  private fail(e: HttpErrorResponse): void {
    this.loading.set(false);
    this.message.set(
      e.status === 403 ? 'nope, your role cannot do that 🚫'
      : e.status === 401 ? 'you got logged out, log in again'
      : e.status === 400 ? 'the server did not like that input, check the fields'
      : 'something went wrong',
    );
  }
}
