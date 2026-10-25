import { Component, computed, inject, signal } from '@angular/core';
import { AuthStore } from '../../core/stores/auth.store';
import { FindingsPanelComponent } from './findings-panel.component';
import { SlaPanelComponent } from './sla-panel.component';
import { RolesPanelComponent } from './roles-panel.component';
import { AuditPanelComponent } from './audit-panel.component';
import {OverviewPanelComponent} from './overview-panel.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FindingsPanelComponent, SlaPanelComponent, RolesPanelComponent, AuditPanelComponent, OverviewPanelComponent],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  protected readonly auth = inject(AuthStore);
  protected readonly tab = signal('overview')

  // only show tabs the role can use (the API checks again)
  protected readonly tabs = computed(() =>
    [
      { id: 'overview', label: 'overview 📊', show: true },
      { id: 'sla', label: 'SLA targets ⏰', show: this.auth.hasRole('pm') },
      { id: 'roles', label: 'user roles 👯', show: this.auth.hasRole('pm') },
      { id: 'audit', label: 'audit log 🔍', show: this.auth.hasRole('auditor') },
    ].filter((t) => t.show),
  );
}
