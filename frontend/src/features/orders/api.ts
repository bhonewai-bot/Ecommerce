import { request } from "../../shared/utils/apiClient";
import type { Order, OrderStatus } from "../../shared/types/api";

export function getOrderByPublicId(publicId: string): Promise<Order> {
  return request(`/api/orders/${publicId}`);
}

export function getOrderByCheckoutSession(sessionId: string): Promise<Order> {
  return request(`/api/orders/by-checkout-session/${sessionId}`);
}

export function cancelOrder(publicId: string): Promise<void> {
  return request(`/api/orders/${publicId}/cancel`, {
    method: "POST",
  });
}

export function updateAdminOrderStatus(
  publicId: string,
  status: OrderStatus,
  idempotencyKey: string
): Promise<void> {
  return request(`/api/admin/orders/${publicId}/status`, {
    method: "PATCH",
    body: { status },
    headers: { "Idempotency-Key": idempotencyKey },
  });
}
