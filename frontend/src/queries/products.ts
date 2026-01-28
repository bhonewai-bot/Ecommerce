import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type {
  CreateProductInput,
  PagedResponse,
  Product,
  UpdateProductInput,
} from "../types/api";
import {
  createProduct,
  getProductById,
  listProducts,
  listPublicProducts,
  removeProduct,
  updateProduct,
} from "../api/products";

type ProductListParams = {
  scope?: "admin" | "public";
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  categoryId?: number | null;
  q?: string;
};

type ProductDetailParams = {
  id: number;
};

export function useProducts(params: ProductListParams) {
  return useQuery({
    queryKey: ["products", params],
    queryFn: () => {
      if (params.scope === "public") {
        const { scope: _scope, ...rest } = params;
        return listPublicProducts(rest);
      }
      const { scope: _scope, ...rest } = params;
      return listProducts(rest);
    },
  });
}

export function useProduct(params: ProductDetailParams) {
  return useQuery({
    queryKey: ["products", "detail", params.id],
    queryFn: () => getProductById(params.id),
    enabled: params.id > 0,
  });
}

export function useCreateProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateProductInput) => createProduct(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
  });
}

export function useUpdateProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: UpdateProductInput }) =>
      updateProduct(id, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => removeProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
    },
  });
}
