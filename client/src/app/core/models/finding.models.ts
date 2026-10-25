// keep these in sync with Severity.cs and FindingStatus.cs in the API, names must match exactly
export const SEVERITIES = ['Low', 'Medium', 'High', 'Critical'];
export const STATUSES = ['New', 'InProgress', 'Resolved', 'Closed'];
export const ROLES = ['analyst', 'developer', 'pm', 'auditor'];

export interface Finding {
  id: string;
  title: string;
  description: string;
  affectedAsset: string;
  severity: string;
  cvssScore: number | null;
  status: string;
  assigneeId?: string | null;
}
