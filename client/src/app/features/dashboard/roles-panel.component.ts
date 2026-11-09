import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { FindingsService } from '../../core/services/findings.service';
import { AppUser, ROLES } from '../../core/models/finding.models';

@Component({
  selector: 'app-roles-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './roles-panel.component.html',
})
export class RolesPanelComponent {
  private readonly api = inject(FindingsService);
  protected readonly roles = ROLES;
  protected readonly users = signal<AppUser[]>([]);
  protected readonly chosen = signal<Set<string>>(new Set());
  protected readonly msg = signal<string | null>(null);
  protected userId = '';

  constructor() {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.api.users().subscribe({
      next: (u) => this.users.set(u),
      error: () => this.msg.set('could not load users'),
    });
  }

  protected pick(id: string): void {
    this.userId = id;
    this.chosen.set(new Set(this.users().find((u) => u.id === id)?.roles ?? []));
    this.msg.set(null);
  }

  protected has(role: string): boolean {
    return this.chosen().has(role);
  }

  protected toggle(role: string, event: Event): void {
    const next = new Set(this.chosen());
    if ((event.target as HTMLInputElement).checked) next.add(role);
    else next.delete(role);
    this.chosen.set(next);
  }

  protected save(): void {
    this.api.setUserRoles(this.userId, [...this.chosen()]).subscribe({
      next: () => {
        this.msg.set('saved, it applies the next time this user logs in');
        this.loadUsers();
      },
      error: (e: HttpErrorResponse) =>
        this.msg.set(e.status === 400 ? (e.error?.error ?? 'not allowed') : 'could not save, only PMs can do this'),
    });
  }
}
