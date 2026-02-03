import { useMutation } from "@tanstack/react-query";
import type { CheckoutRequest, CheckoutResponse } from "../../shared/types/api";
import { createCheckout } from "./api";

export function useCheckout() {
  return useMutation<CheckoutResponse, Error, { input: CheckoutRequest; idempotencyKey: string }>({
    mutationFn: ({ input, idempotencyKey }) => createCheckout(input, idempotencyKey),
  });
}
