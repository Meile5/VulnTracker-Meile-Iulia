import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Finding } from '../models/finding.models';

const API = 'http://localhost:5286';
const withCookie = { withCredentials: true }; // sends the login cookie

@Injectable({ providedIn: 'root' })
export class FindingsService {
  private readonly http = inject(HttpClient);

  list() {
    return this.http.get<Finding[]>(`${API}/api/findings`, withCookie);
  }
  create(body: { title: string; description: string; affectedAsset: string; severity: string; cvssScore: number | null }) {
    return this.http.post<Finding>(`${API}/api/findings`, body, withCookie);
  }
  update(id: string, body: { title?: string; description?: string; status?: string }) {
    return this.http.put<Finding>(`${API}/api/findings/${id}`, body, withCookie);
  }
  assign(id: string, assigneeId: string) {
    return this.http.post<Finding>(`${API}/api/findings/${id}/assign`, { assigneeId }, withCookie);
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
  submitPublic(body: { title: string; description: string; affectedAsset: string; contactEmail: string | null }) {
    return this.http.post(`${API}/api/public/findings`, body);
  }
}
