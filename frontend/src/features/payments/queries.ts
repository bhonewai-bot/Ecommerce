import { useMutation } from "@tanstack/react-query";
import type { PaymentIntentResponse } from "../../shared/types/api";
import { createPaymentIntent } from "./api";

export function useCreatePaymentIntent() {
  return useMutation<PaymentIntentResponse, Error, string>({
    mutationFn: (publicId: string) => createPaymentIntent(publicId),
  });
}
