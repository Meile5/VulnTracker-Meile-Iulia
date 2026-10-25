import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Finding, Summary } from '../models/finding.models';

const API = 'http://localhost:5286';
const withCookie = { withCredentials: true }; // sends the login cookie

@Injectable({ providedIn: 'root' })
export class FindingsService {
  private readonly http = inject(HttpClient);

  // filters match the API query: text, status, severity, asset, assigneeId (empty ones are skipped)
  list(filters: Record<string, string | undefined> = {}) {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(filters)) if (v) params = params.set(k, v);
    return this.http.get<Finding[]>(`${API}/api/findings`, { ...withCookie, params });
  }
  create(body: { title: string; description: string; affectedAsset: string; severity: string; cvssScore: number | null }) {
    return this.http.post<Finding>(`${API}/api/findings`, body, withCookie);
  }
  update(id: string, body: { title?: string; description?: string; status?: string; severity?: string }) {
    return this.http.put<Finding>(`${API}/api/findings/${id}`, body, withCookie);
  }
  assign(id: string, assigneeId: string, dueDate: string | null) {
    return this.http.post<Finding>(`${API}/api/findings/${id}/assign`, { assigneeId, dueDate }, withCookie);
  }
  summary() {
    return this.http.get<Summary>(`${API}/api/reports/summary`, withCookie);
  }
  setSla(severity: string, days: number) {
    return this.http.put(`${API}/api/sla`, { severity, days }, withCookie);
  }
  setUserRoles(userId: string, roles: string[]) {
    return this.http.put(`${API}/api/users/${userId}/roles`, { roles }, withCookie);
  }
  auditLogs() {
    return this.http.get<unknown[]>(`${API}/api/audit-logs`, withCookie);
  }
  // public form, no login needed
  submitPublic(body: { title: string; description: string; affectedAsset: string; contactEmail: string }) {
    return this.http.post<{ reference: string }>(`${API}/api/public/findings`, body);
  }
}
