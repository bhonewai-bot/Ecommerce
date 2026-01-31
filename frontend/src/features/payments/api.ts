import { request } from "../../shared/utils/apiClient";
import type { PaymentIntentResponse } from "../../shared/types/api";

export function createPaymentIntent(publicId: string): Promise<PaymentIntentResponse> {
  return request(`/api/payments/${publicId}/intent`, { method: "POST" });
}
