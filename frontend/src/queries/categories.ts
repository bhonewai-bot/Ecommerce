import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type {
  Category,
  CreateCategoryInput,
  PagedResponse,
  UpdateCategoryInput,
} from "../types/api";
import {
  createCategory,
  listCategories,
  listPublicCategories,
  removeCategory,
  updateCategory,
} from "../api/categories";

type CategoryListParams = {
  page?: number;
  pageSize?: number;
  q?: string;
};

type PublicCategoryParams = {
  scope: "public";
};

export function useCategories(params: CategoryListParams) {
  return useQuery({
    queryKey: ["categories", params],
    queryFn: () => listCategories(params),
  });
}

export function usePublicCategories(params: PublicCategoryParams) {
  return useQuery({
    queryKey: ["categories", params],
    queryFn: () => listPublicCategories(),
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateCategoryInput) => createCategory(input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: UpdateCategoryInput }) =>
      updateCategory(id, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
  });
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => removeCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
  });
}
