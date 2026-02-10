import type { OrderStatus } from "../types/api";

export function getOrderStatusLabel(status: OrderStatus) {
  const labels: Record<OrderStatus, string> = {
    PendingPayment: "Pending payment",
    Paid: "Paid",
    Fulfilled: "Fulfilled",
    Cancelled: "Cancelled",
  };

  return labels[status];
}

export function getOrderStatusMessage(status: OrderStatus) {
  const messages: Record<OrderStatus, string> = {
    PendingPayment: "Waiting for payment...",
    Paid: "Order placed successfully",
    Fulfilled: "Order fulfilled. Thank you for shopping!",
    Cancelled: "Order was cancelled.",
  };

  return messages[status];
}
