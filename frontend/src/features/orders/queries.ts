import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type {
  AdminOrderListItem,
  Order,
  OrderStatus,
  PagedResponse,
} from "../../shared/types/api";
import { getOrderByPublicId, listAdminOrders, updateAdminOrderStatus } from "./api";

type AdminOrderListParams = {
  page?: number;
  pageSize?: number;
  status?: OrderStatus | "all";
};

export function useOrder(publicId?: string) {
  return useQuery<Order>({
    queryKey: ["orders", publicId],
    queryFn: () => getOrderByPublicId(publicId!),
    enabled: Boolean(publicId),
    refetchInterval: (query) =>
      query.state.data?.status === "PendingPayment" ? 3000 : false,
  });
}

export function useAdminOrders(params: AdminOrderListParams) {
  return useQuery<PagedResponse<AdminOrderListItem>>({
    queryKey: ["orders", "admin", params],
    queryFn: () => listAdminOrders(params),
  });
}

export function useUpdateAdminOrderStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: {
      publicId: string;
      status: OrderStatus;
      idempotencyKey: string;
    }) => updateAdminOrderStatus(input.publicId, input.status, input.idempotencyKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
  });
}
