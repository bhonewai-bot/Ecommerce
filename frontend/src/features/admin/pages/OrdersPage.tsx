import { Link } from "react-router-dom";
import { useMemo, useState } from "react";
import type { AdminOrderListItem, OrderStatus } from "../../../shared/types/api";
import { useAdminOrders, useUpdateAdminOrderStatus } from "../../orders/queries";
import { getApiErrorMessage } from "../../../shared/utils/errorMessages";
import StatusBadge from "../../../shared/components/checkout/StatusBadge";
import { getDisplayOrderId } from "../../../shared/utils/orderId";

const statusFilters: Array<{ label: string; value: OrderStatus | "all" }> = [
  { label: "All statuses", value: "all" },
  { label: "Pending payment", value: "PendingPayment" },
  { label: "Paid", value: "Paid" },
  { label: "Fulfilled", value: "Fulfilled" },
  { label: "Cancelled", value: "Cancelled" },
];

function formatMoney(value: number, currency: string) {
  return `${currency} ${Number(value || 0).toFixed(2)}`;
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }
  return date.toLocaleString();
}

function getAvailableTransitions(status: OrderStatus): OrderStatus[] {
  switch (status) {
    case "PendingPayment":
      return ["Cancelled"];
    case "Paid":
      return ["Fulfilled", "Cancelled"];
    default:
      return [];
  }
}

export default function OrdersPage() {
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [status, setStatus] = useState<OrderStatus | "all">("all");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [updatingKey, setUpdatingKey] = useState<string | null>(null);

  const ordersQuery = useAdminOrders({ page, pageSize, status });
  const updateStatusMutation = useUpdateAdminOrderStatus();

  const orders = useMemo<AdminOrderListItem[]>(
    () => ordersQuery.data?.items ?? [],
    [ordersQuery.data]
  );
  const totalCount = ordersQuery.data?.totalCount ?? orders.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const currentPage = Math.min(Math.max(page, 1), totalPages);

  const handleStatusUpdate = (order: AdminOrderListItem, nextStatus: OrderStatus) => {
    setMessage("");
    setError("");
    const key = `${order.publicId}:${nextStatus}`;
    setUpdatingKey(key);

    updateStatusMutation.mutate(
      {
        publicId: order.publicId,
        status: nextStatus,
        idempotencyKey: crypto.randomUUID(),
      },
      {
        onSuccess: () => {
          const label =
            nextStatus === "Fulfilled"
              ? "fulfilled"
              : nextStatus === "Cancelled"
                ? "cancelled"
                : nextStatus.toLowerCase();
          setMessage(`Order ${getDisplayOrderId(order.publicId)} marked as ${label}.`);
        },
        onError: (err) => {
          setError(
            getApiErrorMessage(
              err,
              "Failed to update order status. Please try again."
            )
          );
        },
        onSettled: () => {
          setUpdatingKey(null);
        },
      }
    );
  };

  return (
    <section className="admin-page">
      <div className="admin-panel">
        <div className="admin-panel-header">
          <h3>Orders</h3>
          <button
            className="button ghost"
            type="button"
            onClick={() => ordersQuery.refetch()}
          >
            Refresh
          </button>
        </div>

        <div className="table-toolbar">
          <div className="toolbar-group">
            <label className="toolbar-label">
              Status
              <select
                className="input"
                value={status}
                onChange={(event) => {
                  setStatus(event.target.value as OrderStatus | "all");
                  setPage(1);
                }}
              >
                {statusFilters.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <div className="toolbar-group">
            <label className="toolbar-label">
              Rows
              <select
                className="input"
                value={pageSize}
                onChange={(event) => {
                  setPageSize(Number(event.target.value));
                  setPage(1);
                }}
              >
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
              </select>
            </label>
          </div>
        </div>

        {message && <div className="state admin-success">{message}</div>}
        {error && <div className="state error">{error}</div>}
        {ordersQuery.isLoading && <div className="state">Loading orders…</div>}
        {ordersQuery.isError && !error && (
          <div className="state error">
            {getApiErrorMessage(
              ordersQuery.error,
              "Failed to load orders. Please try again."
            )}
          </div>
        )}

        <div className="table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Status</th>
                <th>Total</th>
                <th>Customer</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.length === 0 && !ordersQuery.isLoading && (
                <tr>
                  <td colSpan={6} className="table-empty">
                    No orders found.
                  </td>
                </tr>
              )}
              {orders.map((order) => {
                const transitions = getAvailableTransitions(order.status);
                return (
                  <tr key={order.publicId}>
                    <td>{getDisplayOrderId(order.publicId)}</td>
                    <td>
                      <StatusBadge status={order.status} />
                    </td>
                    <td>{formatMoney(order.totalAmount, order.currency)}</td>
                    <td className="table-muted">{order.customerEmail || "Guest checkout"}</td>
                    <td className="table-muted">{formatDate(order.createdAt)}</td>
                    <td>
                      <div className="table-actions">
                        <Link className="button ghost" to={`/orders/${order.publicId}`}>
                          View
                        </Link>
                        {transitions.includes("Fulfilled") && (
                          <button
                            className="button primary"
                            type="button"
                            disabled={updatingKey === `${order.publicId}:Fulfilled`}
                            onClick={() => handleStatusUpdate(order, "Fulfilled")}
                          >
                            {updatingKey === `${order.publicId}:Fulfilled`
                              ? "Updating..."
                              : "Fulfill"}
                          </button>
                        )}
                        {transitions.includes("Cancelled") && (
                          <button
                            className="button danger"
                            type="button"
                            disabled={updatingKey === `${order.publicId}:Cancelled`}
                            onClick={() => handleStatusUpdate(order, "Cancelled")}
                          >
                            {updatingKey === `${order.publicId}:Cancelled`
                              ? "Updating..."
                              : "Cancel"}
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        <div className="table-pagination">
          <span>
            Page {currentPage} of {totalPages} · {totalCount} total
          </span>
          <div className="table-actions">
            <button
              className="button ghost"
              type="button"
              onClick={() => setPage((prev) => Math.max(1, prev - 1))}
              disabled={currentPage <= 1}
            >
              Previous
            </button>
            <button
              className="button ghost"
              type="button"
              onClick={() => setPage((prev) => Math.min(totalPages, prev + 1))}
              disabled={currentPage >= totalPages}
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </section>
  );
}
