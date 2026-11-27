interface StatusBadgeProps {
  status: string | number;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const enumLabels: Record<number, string> = {
    0: 'Submitted',
    1: 'Assigned',
    2: 'In Transit',
    3: 'Delivered',
    4: 'Completed',
    5: 'Cancelled',
    6: 'Rejected',
  };
  const label = typeof status === 'number' ? enumLabels[status] ?? 'Unknown' : status?.trim() || 'Pending';
  const normalized = label.replace(/[\s_-]/g, '').toLowerCase();

  const toneMap: Record<string, string> = {
    submitted: 'status-warning',
    assigned: 'status-assigned',
    intransit: 'status-warning',
    delivered: 'status-success',
    completed: 'status-success',
    cancelled: 'status-danger',
    rejected: 'status-danger',
    pending: 'status-warning',
    confirmed: 'status-success',
  };

  return <span className={`status-badge ${toneMap[normalized] ?? 'status-default'}`}>{label}</span>;
}
