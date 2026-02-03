import { useMutation, useQuery } from "@tanstack/react-query";
import type { Order, OrderStatus } from "../../shared/types/api";
import { getOrderByPublicId, updateAdminOrderStatus } from "./api";

export function useOrder(publicId?: string) {
  return useQuery<Order>({
    queryKey: ["orders", publicId],
    queryFn: () => getOrderByPublicId(publicId!),
    enabled: Boolean(publicId),
    refetchInterval: (query) =>
      query.state.data?.status === "PendingPayment" ? 3000 : false,
  });
}

export function useUpdateAdminOrderStatus() {
  return useMutation({
    mutationFn: (input: {
      publicId: string;
      status: OrderStatus;
      idempotencyKey: string;
    }) => updateAdminOrderStatus(input.publicId, input.status, input.idempotencyKey),
  });
}
