import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FindingsService } from '../../core/services/findings.service';
import { ROLES } from '../../core/models/finding.models';

@Component({
  selector: 'app-roles-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './roles-panel.component.html',
})
export class RolesPanelComponent {
  private readonly api = inject(FindingsService);
  protected readonly roles = ROLES;
  protected userId = '';
  private readonly chosen = new Set<string>();
  protected readonly msg = signal<string | null>(null);

  protected toggle(role: string, event: Event) {
    const on = (event.target as HTMLInputElement).checked;
    if (on) this.chosen.add(role);
    else this.chosen.delete(role);
  }

  protected save() {
    this.api.setUserRoles(this.userId.trim(), [...this.chosen]).subscribe({
      next: () => this.msg.set('saved ✨ (stub for now, later it should call the Keycloak admin API)'),
      error: () => this.msg.set('could not save, only PMs can do this'),
    });
  }
}
