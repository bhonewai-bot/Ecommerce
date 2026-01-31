import { request } from "../../shared/utils/apiClient";
import type {
  Product,
  CreateProductInput,
  UpdateProductInput,
  PagedResponse,
} from "../../shared/types/api";

export function listProducts(params: {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  categoryId?: number | null;
  q?: string;
}): Promise<PagedResponse<Product>> {
  return request("/api/admin/products", { params });
}

export function listPublicProducts(params: {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  categoryId?: number | null;
}): Promise<PagedResponse<Product>> {
  return request("/api/products", { params });
}

export function getProductById(id: number): Promise<Product> {
  return request(`/api/products/${id}`);
}

export function createProduct(input: CreateProductInput): Promise<Product> {
  return request("/api/admin/products", { method: "POST", body: input });
}

export function updateProduct(id: number, input: UpdateProductInput): Promise<void> {
  return request(`/api/admin/products/${id}`, { method: "PUT", body: input });
}

export function removeProduct(id: number): Promise<void> {
  return request(`/api/admin/products/${id}`, { method: "DELETE" });
}
