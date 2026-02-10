import { mapErrorCode } from "./errorMessages";

type RequestOptions = {
  method?: string;
  body?: unknown;
  params?: Record<string, string | number | boolean | null | undefined>;
  headers?: Record<string, string>;
};

const API_URL =
  import.meta.env.VITE_API_URL?.replace(/\/$/, "") ||
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ||
  "http://localhost:5050";

function buildQuery(params?: RequestOptions["params"]) {
  if (!params) return "";
  const searchParams = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value === null || value === undefined) return;
    searchParams.set(key, String(value));
  });
  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

export async function request<T>(
  path: string,
  { method = "GET", body, params, headers = {} }: RequestOptions = {}
): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}${buildQuery(params)}`, {
      method,
      headers: {
        "Content-Type": "application/json",
        ...headers,
      },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    const error = new Error(mapErrorCode("NETWORK_ERROR")) as Error & {
      status?: number;
      payload?: unknown;
      code?: string;
    };
    error.code = "NETWORK_ERROR";
    throw error;
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const isJson = response.headers
    .get("content-type")
    ?.includes("application/json");
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const code =
      payload && typeof payload === "object" && "code" in payload
        ? String((payload as { code?: string }).code ?? "")
        : undefined;
    const message = mapErrorCode(code) ?? response.statusText;
    const error = new Error(message) as Error & {
      status?: number;
      payload?: unknown;
      code?: string;
    };
    error.status = response.status;
    error.payload = payload;
    error.code = code || "REQUEST_FAILED";
    throw error;
  }

  return payload as T;
}
