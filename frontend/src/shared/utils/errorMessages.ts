type ApiErrorPayload = {
  code?: string;
  message?: string;
};

const ERROR_MESSAGES: Record<string, string> = {
  BAD_REQUEST: "Please check your input and try again.",
  VALIDATION_FAILED: "Please check the form and try again.",
  NOT_FOUND: "We couldn't find what you're looking for.",
  CONFLICT: "This request conflicts with the current state. Please try again.",
  IDEMPOTENCY_KEY_REQUIRED: "Please refresh and try again.",
  UNEXPECTED_ERROR: "Something went wrong. Please try again.",
  NETWORK_ERROR: "We couldn't reach the server. Please try again.",
  REQUEST_FAILED: "Something went wrong. Please try again.",
};

export function mapErrorCode(code?: string) {
  if (!code) return undefined;
  return ERROR_MESSAGES[code] ?? ERROR_MESSAGES.REQUEST_FAILED;
}

export function getApiErrorCode(error: unknown): string | undefined {
  if (!error || typeof error !== "object") return undefined;
  const withCode = error as { code?: string; payload?: ApiErrorPayload };
  return withCode.code ?? withCode.payload?.code;
}

export function getApiErrorMessage(error: unknown, fallback?: string) {
  if (!error) return fallback;
  const code = getApiErrorCode(error);
  if (code) {
    return mapErrorCode(code) ?? fallback ?? ERROR_MESSAGES.REQUEST_FAILED;
  }
  return fallback ?? ERROR_MESSAGES.REQUEST_FAILED;
}
