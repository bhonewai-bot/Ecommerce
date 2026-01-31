import { request } from "../../shared/utils/apiClient";
import type {
  Category,
  CreateCategoryInput,
  UpdateCategoryInput,
  PagedResponse,
} from "../../shared/types/api";

export function listCategories(params?: {
  page?: number;
  pageSize?: number;
  q?: string;
}): Promise<PagedResponse<Category>> {
  return request("/api/admin/categories", { params });
}

export function listPublicCategories(): Promise<Category[]> {
  return request("/api/categories");
}

export function createCategory(input: CreateCategoryInput): Promise<Category> {
  return request("/api/admin/categories", { method: "POST", body: input });
}

export function updateCategory(id: number, input: UpdateCategoryInput): Promise<void> {
  return request(`/api/admin/categories/${id}`, { method: "PUT", body: input });
}

export function removeCategory(id: number): Promise<void> {
  return request(`/api/admin/categories/${id}`, { method: "DELETE" });
}
