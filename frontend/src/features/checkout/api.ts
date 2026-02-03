import { request } from "../../shared/utils/apiClient";
import type { CheckoutRequest, CheckoutResponse } from "../../shared/types/api";

export function createCheckout(
  input: CheckoutRequest,
  idempotencyKey: string
): Promise<CheckoutResponse> {
  return request("/api/checkout", {
    method: "POST",
    body: input,
    headers: { "Idempotency-Key": idempotencyKey },
  });
}
