import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { AuthStore } from '../../core/stores/auth.store';
import { FindingsService } from '../../core/services/findings.service';
import { Finding, SEVERITIES, STATUSES } from '../../core/models/finding.models';

type Mode = 'create' | 'edit' | 'assign';
type TagSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary';

const blankForm = () => ({
  title: '', description: '', affectedAsset: '', severity: 'Medium',
  cvssScore: null as number | null, status: 'New', assigneeId: '',
});

@Component({
  selector: 'app-findings-panel',
  standalone: true,
  imports: [FormsModule, TableModule, TagModule],
  template: `
    <div class="pink-card">
      <div class="flex flex-wrap items-center gap-3 mb-4">
        <input class="pink-input max-w-xs" placeholder="search title or asset 🔍"
               [ngModel]="search()" (ngModelChange)="search.set($event)" />
        <span class="flex-1"></span>
        @if (canCreate()) {
          <button class="pink-btn" (click)="openCreate()">+ new finding</button>
        }
      </div>

      @if (message() && !mode()) { <p class="pink-error">{{ message() }}</p> }

      <p-table [value]="visible()" [paginator]="true" [rows]="8" [loading]="loading()" dataKey="id">
        <ng-template #header>
          <tr>
            <th>Title</th><th>Asset</th><th>Severity</th><th>CVSS</th>
            <th>Status</th><th>Assignee</th><th></th>
          </tr>
        </ng-template>
        <ng-template #body let-f>
          <tr>
            <td>{{ f.title }}</td>
            <td>{{ f.affectedAsset }}</td>
            <td><p-tag [value]="f.severity" [severity]="tagSeverity(f.severity)" /></td>
            <td>{{ f.cvssScore ?? '-' }}</td>
            <td>{{ f.status }}</td>
            <td>{{ f.assigneeId ? (f.assigneeId === auth.user()?.id ? 'you' : 'assigned') : '-' }}</td>
            <td class="flex gap-2 justify-end">
              @if (canEdit(f)) { <button class="pink-btn-ghost" (click)="openEdit(f)">edit</button> }
              @if (canAssign()) { <button class="pink-btn-ghost" (click)="openAssign(f)">assign</button> }
            </td>
          </tr>
        </ng-template>
        <ng-template #emptymessage>
          <tr><td colspan="7">no findings yet ✨</td></tr>
        </ng-template>
      </p-table>
    </div>

    @if (mode(); as m) {
      <div class="modal-backdrop" (click)="close()">
        <div class="pink-card modal" (click)="$event.stopPropagation()">
          <h2 class="pink-title text-2xl mb-2">
            {{ m === 'create' ? 'new finding' : m === 'edit' ? 'edit finding' : 'assign finding' }}
          </h2>

          @if (m !== 'assign') {
            <label class="pink-label">title</label>
            <input class="pink-input" [(ngModel)]="form.title" />
            <label class="pink-label">description</label>
            <textarea class="pink-input" rows="3" [(ngModel)]="form.description"></textarea>
          }
          @if (m === 'create') {
            <label class="pink-label">affected asset</label>
            <input class="pink-input" [(ngModel)]="form.affectedAsset" />
            <label class="pink-label">severity</label>
            <select class="pink-input" [(ngModel)]="form.severity">
              @for (s of severities; track s) { <option [value]="s">{{ s }}</option> }
            </select>
            <label class="pink-label">CVSS score (optional)</label>
            <input type="number" min="0" max="10" step="0.1" class="pink-input" [(ngModel)]="form.cvssScore" />
          }
          @if (m === 'edit') {
            <label class="pink-label">status</label>
            <select class="pink-input" [(ngModel)]="form.status">
              @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
            </select>
          }
          @if (m === 'assign') {
            <label class="pink-label">Keycloak user ID</label>
            <input class="pink-input" [(ngModel)]="form.assigneeId" placeholder="paste a user id" />
            <button class="pink-btn-ghost self-start" (click)="assignToMe()">assign to me</button>
          }

          @if (message()) { <p class="pink-error">{{ message() }}</p> }

          <div class="flex justify-end gap-2 mt-3">
            <button class="pink-btn-ghost" (click)="close()">cancel</button>
            <button class="pink-btn" (click)="save()">save 💖</button>
          </div>
        </div>
      </div>
    }
  `,
})
export class FindingsPanelComponent {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(FindingsService);

  protected readonly severities = SEVERITIES;
  protected readonly statuses = STATUSES;

  protected readonly findings = signal<Finding[]>([]);
  protected readonly search = signal('');
  protected readonly loading = signal(false);
  protected readonly message = signal<string | null>(null);
  protected readonly mode = signal<Mode | null>(null);
  protected readonly selected = signal<Finding | null>(null);
  protected form = blankForm();

  protected readonly visible = computed(() => {
    const q = this.search().toLowerCase();
    return this.findings().filter(
      (f) => f.title.toLowerCase().includes(q) || f.affectedAsset.toLowerCase().includes(q),
    );
  });

  constructor() {
    this.load();
  }

  // same rules as the API policies, only used to hide buttons (the API still checks)
  protected canCreate() { return this.auth.hasRole('analyst') || this.auth.hasRole('pm'); }
  protected canAssign() { return this.canCreate(); }
  protected canEdit(f: Finding) {
    if (this.auth.hasRole('analyst') || this.auth.hasRole('pm')) return true;
    // developers only on findings assigned to them
    return this.auth.hasRole('developer') && !!f.assigneeId && f.assigneeId === this.auth.user()?.id;
  }

  protected tagSeverity(s: string): TagSeverity {
    return ({ Critical: 'danger', High: 'warn', Medium: 'info', Low: 'success' } as Record<string, TagSeverity>)[s] ?? 'secondary';
  }

  protected load(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (f) => { this.findings.set(f); this.loading.set(false); },
      error: (e) => this.fail(e),
    });
  }

  protected openCreate() { this.form = blankForm(); this.message.set(null); this.mode.set('create'); }
  protected openEdit(f: Finding) {
    this.selected.set(f);
    this.form = { ...blankForm(), title: f.title, description: f.description, status: f.status };
    this.message.set(null);
    this.mode.set('edit');
  }
  protected openAssign(f: Finding) {
    this.selected.set(f);
    this.form = { ...blankForm(), assigneeId: f.assigneeId ?? '' };
    this.message.set(null);
    this.mode.set('assign');
  }
  protected assignToMe() { this.form.assigneeId = this.auth.user()?.id ?? ''; }
  protected close() { this.mode.set(null); }

  protected save(): void {
    const f = this.selected();
    const x = this.form;
    const req =
      this.mode() === 'create'
        ? this.api.create({ title: x.title, description: x.description, affectedAsset: x.affectedAsset, severity: x.severity, cvssScore: x.cvssScore })
        : this.mode() === 'edit' && f
          ? this.api.update(f.id, { title: x.title, description: x.description, status: x.status })
          : this.mode() === 'assign' && f
            ? this.api.assign(f.id, x.assigneeId.trim())
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
