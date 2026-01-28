export interface Category {
  id: number;
  name: string;
  description?: string | null;
}

export interface Product {
  id: number;
  categoryId: number;
  name: string;
  description?: string | null;
  price: number;
  imageUrl?: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export type CreateProductInput = Omit<Product, "id">;
export type UpdateProductInput = Omit<Product, "id">;
export type CreateCategoryInput = Omit<Category, "id">;
export type UpdateCategoryInput = Omit<Category, "id">;
