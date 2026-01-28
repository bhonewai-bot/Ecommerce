type RequestOptions = {
  method?: string;
  body?: unknown;
  params?: Record<string, string | number | boolean | null | undefined>;
  headers?: Record<string, string>;
};

const API_URL =
  import.meta.env.VITE_API_URL?.replace(/\/$/, "") ||
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ||
  "http://localhost:5000";

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
  const response = await fetch(`${API_URL}${path}${buildQuery(params)}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...headers,
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const isJson = response.headers
    .get("content-type")
    ?.includes("application/json");
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const message = typeof payload === "string" ? payload : response.statusText;
    const error = new Error(message) as Error & {
      status?: number;
      payload?: unknown;
    };
    error.status = response.status;
    error.payload = payload;
    throw error;
  }

  return payload as T;
}
