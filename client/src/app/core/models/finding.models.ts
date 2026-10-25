// names must match the enums in the API exactly
export const SEVERITIES = ['Low', 'Medium', 'High', 'Critical'];
export const STATUSES = ['New', 'Triaged', 'InProgress', 'Remediated', 'AcceptedRisk'];
export const ROLES = ['analyst', 'developer', 'pm', 'auditor'];

// "InProgress" -> "In Progress"
export const statusLabel = (s: string) => s.replace(/([a-z])([A-Z])/g, '$1 $2');

export interface Finding {
  id: string;
  title: string;
  description: string;
  affectedAsset: string;
  severity: string;
  cvssScore: number | null;
  status: string;
  source?: string;
  assigneeId?: string | null;
  dueDate?: string | null;
}

export interface Summary {
  open: number;
  overdue: number;
  openBySeverity: Record<string, number>;
  byStatus: Record<string, number>;
}
