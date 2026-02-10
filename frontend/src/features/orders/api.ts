import { request } from "../../shared/utils/apiClient";
import type {
  AdminOrderListItem,
  Order,
  OrderStatus,
  PagedResponse,
} from "../../shared/types/api";

export interface CheckoutResumeResponse {
  orderPublicId: string;
  checkoutUrl: string;
}

export function getOrderByPublicId(publicId: string): Promise<Order> {
  return request(`/api/orders/${publicId}`);
}

export function getOrderByCheckoutSession(sessionId: string): Promise<Order> {
  return request(`/api/orders/by-checkout-session/${sessionId}`);
}

export function getResumableCheckoutBySession(
  sessionId: string
): Promise<CheckoutResumeResponse> {
  return request(`/api/orders/by-checkout-session/${sessionId}/resume`);
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

type ListAdminOrdersParams = {
  page?: number;
  pageSize?: number;
  status?: OrderStatus | "all";
};

export function listAdminOrders(
  params: ListAdminOrdersParams = {}
): Promise<PagedResponse<AdminOrderListItem>> {
  return request("/api/admin/orders", {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 10,
      status: params.status === "all" ? undefined : params.status,
    },
  });
}
