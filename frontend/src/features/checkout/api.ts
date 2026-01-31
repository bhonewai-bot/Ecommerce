import { request } from "../../shared/utils/apiClient";
import type { CheckoutRequest, CheckoutResponse } from "../../shared/types/api";

export function createCheckout(input: CheckoutRequest): Promise<CheckoutResponse> {
  return request("/api/checkout", { method: "POST", body: input });
}
