import { useMemo, useRef, useState } from "react";
import type { FormEvent } from "react";
import ConfirmDialog from "../components/ConfirmDialog";
import type { Category, Product, CreateProductInput } from "../../../shared/types/api";
import {
  useCreateProduct,
  useDeleteProduct,
  useProducts,
  useUpdateProduct,
} from "../../products/queries";
import { useCategories } from "../../categories/queries";
import { getApiErrorMessage } from "../../../shared/utils/errorMessages";

const emptyProductForm = {
  name: "",
  description: "",
  price: Number.NaN,
  imageUrl: "",
  categoryId: "",
};

type ProductFormState = typeof emptyProductForm;

export default function ProductsPage() {
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [productForm, setProductForm] = useState<ProductFormState>(
    emptyProductForm
  );
  const [editingId, setEditingId] = useState<number | null>(null);
  const [pendingDelete, setPendingDelete] = useState<Product | null>(null);
  const formRef = useRef<HTMLFormElement | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const categoriesQuery = useCategories({ page: 1, pageSize: 200 });
  const productsQuery = useProducts({
    page,
    pageSize,
    sortBy: "id",
    sortOrder: "desc",
    q: searchQuery,
  });

  const categories = useMemo<Category[]>(() => {
    return categoriesQuery.data?.items ?? [];
  }, [categoriesQuery.data]);

  const products = productsQuery.data?.items ?? [];
  const totalCount = productsQuery.data?.totalCount ?? products.length;

  const categoryMap = useMemo<Record<number, string>>(() => {
    return Object.fromEntries(categories.map((cat) => [cat.id, cat.name]));
  }, [categories]);

  const resetForm = () => {
    setProductForm(emptyProductForm);
    setEditingId(null);
  };

  const handleEdit = (product: Product) => {
    setEditingId(product.id);
    setProductForm({
      name: product.name ?? "",
      description: product.description ?? "",
      price: Number.isFinite(product.price) ? product.price : Number.NaN,
      imageUrl: product.imageUrl ?? "",
      categoryId: product.categoryId?.toString() ?? "",
    });
    setMessage("");
    setError("");
    requestAnimationFrame(() => {
      formRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  };

  const createMutation = useCreateProduct();
  const updateMutation = useUpdateProduct();
  const deleteMutation = useDeleteProduct();

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage("");
    setError("");

    const normalizedPrice = Number.isFinite(productForm.price)
      ? productForm.price
      : 0;
    const payload: CreateProductInput = {
      name: productForm.name.trim(),
      description: productForm.description.trim() || null,
      price: normalizedPrice,
      imageUrl: productForm.imageUrl.trim() || null,
      categoryId: Number(productForm.categoryId),
    };

    if (!payload.name) {
      setError("Product name is required.");
      return;
    }

    if (!payload.categoryId) {
      setError("Select a category for the product.");
      return;
    }

    if (payload.price < 0) {
      setError("Price must be 0 or greater.");
      return;
    }

    if (editingId) {
      updateMutation.mutate(
        { id: editingId, input: payload },
        {
          onSuccess: () => {
            setMessage("Product updated.");
            resetForm();
          },
          onError: (err) => {
            setError(
              getApiErrorMessage(err, "Failed to save product. Please try again.")
            );
          },
        }
      );
      return;
    }

    createMutation.mutate(payload, {
      onSuccess: () => {
        setMessage("Product created.");
        resetForm();
      },
      onError: (err) => {
        setError(
          getApiErrorMessage(err, "Failed to save product. Please try again.")
        );
      },
    });
  };

  const handleDelete = async (product: Product) => {
    setMessage("");
    setError("");
    deleteMutation.mutate(product.id, {
      onSuccess: () => {
        setMessage("Product deleted.");
        if (editingId === product.id) {
          resetForm();
        }
      },
      onError: (err) => {
        setError(
          getApiErrorMessage(err, "Failed to delete product. Please try again.")
        );
      },
    });
  };

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const currentPage = Math.min(Math.max(page, 1), totalPages);

  return (
    <section className="admin-page">
      <div className="admin-panel">
        <div className="admin-panel-header">
          <h3>Products</h3>
          <button
            className="button ghost"
            type="button"
            onClick={() => productsQuery.refetch()}
          >
            Refresh
          </button>
        </div>
        <form className="admin-form-card" onSubmit={handleSubmit} ref={formRef}>
          <div className="form-grid">
            <div className="form-row">
              <label htmlFor="product-name">Name</label>
              <input
                id="product-name"
                className="input"
                type="text"
                value={productForm.name}
                onChange={(event) =>
                  setProductForm((prev) => ({
                    ...prev,
                    name: event.target.value,
                  }))
                }
                placeholder="Product name"
                required
              />
            </div>
            <div className="form-row">
              <label htmlFor="product-category">Category</label>
              <select
                id="product-category"
                className="input"
                value={productForm.categoryId}
                onChange={(event) =>
                  setProductForm((prev) => ({
                    ...prev,
                    categoryId: event.target.value,
                  }))
                }
                required
              >
                <option value="">Select a category</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-row">
              <label htmlFor="product-price">Price</label>
              <input
                id="product-price"
                className="input"
                type="number"
                min="0"
                step="0.01"
                value={Number.isFinite(productForm.price) ? productForm.price : ""}
                onChange={(event) =>
                  setProductForm((prev) => ({
                    ...prev,
                    price:
                      event.target.value === ""
                        ? Number.NaN
                        : Number(event.target.value),
                  }))
                }
                placeholder="0.00"
              />
            </div>
            <div className="form-row">
              <label htmlFor="product-image">Image URL</label>
              <input
                id="product-image"
                className="input"
                type="text"
                value={productForm.imageUrl}
                onChange={(event) =>
                  setProductForm((prev) => ({
                    ...prev,
                    imageUrl: event.target.value,
                  }))
                }
                placeholder="https://"
              />
            </div>
          </div>
          <div className="form-row">
            <label htmlFor="product-description">Description</label>
            <textarea
              id="product-description"
              className="input textarea"
              rows={3}
              value={productForm.description}
              onChange={(event) =>
                setProductForm((prev) => ({
                  ...prev,
                  description: event.target.value,
                }))
              }
              placeholder="Short product summary."
            />
          </div>
          <div className="form-actions">
            <button className="button primary" type="submit">
              {editingId ? "Update product" : "Add product"}
            </button>
            {editingId ? (
              <button className="button ghost" type="button" onClick={resetForm}>
                Cancel edit
              </button>
            ) : (
              <button className="button ghost" type="button" onClick={resetForm}>
                Clear
              </button>
            )}
          </div>
        </form>
        <div className="table-toolbar">
          <div className="toolbar-group">
            <input
              className="input"
              type="search"
              placeholder="Search products"
              value={searchQuery}
              onChange={(event) => {
                setSearchQuery(event.target.value);
                setPage(1);
              }}
            />
          </div>
          <div className="toolbar-group">
            <label className="toolbar-label">
              Rows
              <select
                className="input"
                value={pageSize}
                onChange={(event) => {
                  setPageSize(Number(event.target.value));
                  setPage(1);
                }}
              >
                <option value={5}>5</option>
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
              </select>
            </label>
          </div>
        </div>

        {message && <div className="state admin-success">{message}</div>}
        {error && <div className="state error">{error}</div>}
        {productsQuery.isLoading && (
          <div className="state">Loading products…</div>
        )}
        {productsQuery.isError && !error && (
          <div className="state error">
            {getApiErrorMessage(
              productsQuery.error,
              "Failed to load products. Please try again."
            )}
          </div>
        )}

        <div className="table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Category</th>
                <th>Price</th>
                <th>Description</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.length === 0 && !productsQuery.isLoading && (
                <tr>
                  <td colSpan={6} className="table-empty">
                    No products yet.
                  </td>
                </tr>
              )}
              {products.map((product) => (
                <tr key={product.id}>
                  <td>{product.id}</td>
                  <td>{product.name}</td>
                  <td>{categoryMap[product.categoryId] || "Unassigned"}</td>
                  <td>${Number(product.price || 0).toFixed(2)}</td>
                  <td className="table-muted">{product.description || "-"}</td>
                  <td>
                    <div className="table-actions">
                      <button
                        className="button ghost"
                        type="button"
                        onClick={() => handleEdit(product)}
                      >
                        Edit
                      </button>
                      <button
                        className="button ghost"
                        type="button"
                        onClick={() => setPendingDelete(product)}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="table-pagination">
          <span>
            Page {currentPage} of {totalPages} · {totalCount} total
          </span>
          <div className="table-actions">
            <button
              className="button ghost"
              type="button"
              onClick={() => setPage((prev) => Math.max(1, prev - 1))}
              disabled={currentPage <= 1}
            >
              Previous
            </button>
            <button
              className="button ghost"
              type="button"
              onClick={() => setPage((prev) => Math.min(totalPages, prev + 1))}
              disabled={currentPage >= totalPages}
            >
              Next
            </button>
          </div>
        </div>
      </div>
      <ConfirmDialog
        open={Boolean(pendingDelete)}
        title="Delete product?"
        description={
          pendingDelete
            ? `This will remove "${pendingDelete.name}" from the catalog.`
            : ""
        }
        confirmLabel="Delete product"
        onCancel={() => setPendingDelete(null)}
        onConfirm={async () => {
          if (!pendingDelete) return;
          const target = pendingDelete;
          setPendingDelete(null);
          await handleDelete(target);
        }}
      />
    </section>
  );
}
