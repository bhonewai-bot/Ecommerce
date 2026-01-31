import { useQuery } from "@tanstack/react-query";
import type { Order } from "../../shared/types/api";
import { getOrderByPublicId } from "./api";

export function useOrder(publicId?: string) {
  return useQuery<Order>({
    queryKey: ["orders", publicId],
    queryFn: () => getOrderByPublicId(publicId!),
    enabled: Boolean(publicId),
    refetchInterval: (query) =>
      query.state.data?.status === "PendingPayment" ? 3000 : false,
  });
}
