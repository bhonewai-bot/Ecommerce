const API_BASE =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") || "http://localhost:5000";

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
    ...options,
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const api = {
  getCategories: () => request("/api/categories"),
  getProducts: () => request("/api/products"),
  getProduct: (id) => request(`/api/products/${id}`),
};
