import { useState } from "react";
import type { ReactNode } from "react";

export interface SummaryRow {
  label: string;
  value: string;
}

interface OrderSummaryCardProps {
  title?: string;
  rows: SummaryRow[];
  totalLabel: string;
  totalValue: string;
  note?: string;
  cta?: ReactNode;
  footer?: ReactNode;
  compactOnMobile?: boolean;
}

export default function OrderSummaryCard({
  title = "Order summary",
  rows,
  totalLabel,
  totalValue,
  note,
  cta,
  footer,
  compactOnMobile = true,
}: OrderSummaryCardProps) {
  const [collapsed, setCollapsed] = useState(compactOnMobile);

  return (
    <aside className="order-summary-card">
      <div className="order-summary-card-head">
        <h3>{title}</h3>
        {compactOnMobile && (
          <button
            className="button ghost order-summary-toggle"
            type="button"
            aria-expanded={!collapsed}
            onClick={() => setCollapsed((value) => !value)}
          >
            {collapsed ? "Show" : "Hide"}
          </button>
        )}
      </div>
      <div className={`order-summary-body${collapsed ? " is-collapsed" : ""}`}>
        {rows.map((row) => (
          <div key={`${row.label}-${row.value}`} className="order-summary-row">
            <span>{row.label}</span>
            <strong>{row.value}</strong>
          </div>
        ))}
        <div className="order-summary-row order-summary-total">
          <span>{totalLabel}</span>
          <strong>{totalValue}</strong>
        </div>
        {note && <p className="order-summary-note">{note}</p>}
        {cta}
        {footer}
      </div>
    </aside>
  );
}
