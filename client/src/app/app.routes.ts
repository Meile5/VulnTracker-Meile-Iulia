import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ReportComponent } from './features/report/report.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'report', component: ReportComponent }, // public, no guard
  { path: 'home', component: DashboardComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'home', pathMatch: 'full' },
];
