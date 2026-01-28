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
  getAdminCategories: (page = 1, pageSize = 10, q = "") => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (q) {
      params.set("q", q);
    }
    return request(`/api/admin/categories?${params.toString()}`);
  },
  createCategory: (payload) =>
    request("/api/admin/categories", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updateCategory: (id, payload) =>
    request(`/api/admin/categories/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),
  deleteCategory: (id) =>
    request(`/api/admin/categories/${id}`, {
      method: "DELETE",
    }),
  getProducts: (
    page = 1,
    pageSize = 10,
    sortBy = "id",
    sortOrder = "asc",
    categoryId = null
  ) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortBy,
      sortOrder,
    });
    if (categoryId) {
      params.set("categoryId", categoryId.toString());
    }
    return request(`/api/products?${params.toString()}`);
  },
  getAdminProducts: (
    page = 1,
    pageSize = 10,
    sortBy = "id",
    sortOrder = "asc",
    categoryId = null,
    q = ""
  ) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortBy,
      sortOrder,
    });
    if (categoryId) {
      params.set("categoryId", categoryId.toString());
    }
    if (q) {
      params.set("q", q);
    }
    return request(`/api/admin/products?${params.toString()}`);
  },
  getProduct: (id) => request(`/api/products/${id}`),
  createProduct: (payload) =>
    request("/api/admin/products", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updateProduct: (id, payload) =>
    request(`/api/admin/products/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),
  deleteProduct: (id) =>
    request(`/api/admin/products/${id}`, {
      method: "DELETE",
    }),
};
