import { useMutation } from "@tanstack/react-query";
import type { PaymentIntentResponse } from "../../shared/types/api";
import { createPaymentIntent } from "./api";

export function useCreatePaymentIntent() {
  return useMutation<PaymentIntentResponse, Error, { publicId: string; idempotencyKey: string }>({
    mutationFn: ({ publicId, idempotencyKey }) => createPaymentIntent(publicId, idempotencyKey),
  });
}
