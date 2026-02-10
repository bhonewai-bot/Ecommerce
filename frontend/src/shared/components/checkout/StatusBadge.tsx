interface StatusBadgeProps {
  status: string;
}

const labelMap: Record<string, string> = {
  Paid: "Paid",
  PendingPayment: "Pending payment",
  Processing: "Processing",
  Failed: "Failed",
  Fulfilled: "Fulfilled",
  Cancelled: "Cancelled",
};

const toneMap: Record<string, string> = {
  Paid: "is-paid",
  PendingPayment: "is-pending",
  Processing: "is-processing",
  Failed: "is-failed",
  Fulfilled: "is-paid",
  Cancelled: "is-failed",
};

export default function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span className={`status-badge ${toneMap[status] ?? "is-pending"}`}>
      {labelMap[status] ?? status}
    </span>
  );
}
