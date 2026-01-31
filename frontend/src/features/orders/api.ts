import { request } from "../../shared/utils/apiClient";
import type { Order } from "../../shared/types/api";

export function getOrderByPublicId(publicId: string): Promise<Order> {
  return request(`/api/orders/${publicId}`);
}
