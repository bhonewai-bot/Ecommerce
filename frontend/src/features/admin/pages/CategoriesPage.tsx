import { useRef, useState } from "react";
import type { FormEvent } from "react";
import ConfirmDialog from "../components/ConfirmDialog";
import type { Category, CreateCategoryInput } from "../../../shared/types/api";
import {
  useCategories,
  useCreateCategory,
  useDeleteCategory,
  useUpdateCategory,
} from "../../categories/queries";
import { getApiErrorMessage } from "../../../shared/utils/errorMessages";

const emptyCategoryForm = { name: "", description: "" };
type CategoryFormState = typeof emptyCategoryForm;

export default function CategoriesPage() {
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [categoryForm, setCategoryForm] = useState<CategoryFormState>(
    emptyCategoryForm
  );
  const [editingId, setEditingId] = useState<number | null>(null);
  const [pendingDelete, setPendingDelete] = useState<Category | null>(null);
  const formRef = useRef<HTMLFormElement | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const categoriesQuery = useCategories({ page, pageSize, q: searchQuery });

  const categories = categoriesQuery.data?.items ?? [];
  const totalCount = categoriesQuery.data?.totalCount ?? categories.length;

  const resetForm = () => {
    setCategoryForm(emptyCategoryForm);
    setEditingId(null);
  };

  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const deleteMutation = useDeleteCategory();

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage("");
    setError("");

    const payload: CreateCategoryInput = {
      name: categoryForm.name.trim(),
      description: categoryForm.description.trim() || null,
    };

    if (!payload.name) {
      setError("Category name is required.");
      return;
    }

    if (editingId) {
      updateMutation.mutate(
        { id: editingId, input: payload },
        {
          onSuccess: () => {
            setMessage("Category updated.");
            resetForm();
          },
          onError: (err) => {
            setError(
              getApiErrorMessage(err, "Failed to save category. Please try again.")
            );
          },
        }
      );
      return;
    }

    createMutation.mutate(payload, {
      onSuccess: () => {
        setMessage("Category created.");
        resetForm();
      },
      onError: (err) => {
        setError(
          getApiErrorMessage(err, "Failed to save category. Please try again.")
        );
      },
    });
  };

  const handleEdit = (category: Category) => {
    setEditingId(category.id);
    setCategoryForm({
      name: category.name ?? "",
      description: category.description ?? "",
    });
    setMessage("");
    setError("");
    requestAnimationFrame(() => {
      formRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  };

  const handleDelete = async (category: Category) => {
    setMessage("");
    setError("");
    deleteMutation.mutate(category.id, {
      onSuccess: () => {
        setMessage("Category deleted.");
        if (editingId === category.id) {
          resetForm();
        }
      },
      onError: (err) => {
        setError(
          getApiErrorMessage(err, "Failed to delete category. Please try again.")
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
          <h3>Categories</h3>
          <button
            className="button ghost"
            type="button"
            onClick={() => categoriesQuery.refetch()}
          >
            Refresh
          </button>
        </div>
        <form className="admin-form-card" onSubmit={handleSubmit} ref={formRef}>
          <div className="form-grid">
            <div className="form-row">
              <label htmlFor="category-name">Name</label>
              <input
                id="category-name"
                className="input"
                type="text"
                value={categoryForm.name}
                onChange={(event) =>
                  setCategoryForm((prev) => ({
                    ...prev,
                    name: event.target.value,
                  }))
                }
                placeholder="Category name"
                required
              />
            </div>
            <div className="form-row">
              <label htmlFor="category-description">Description</label>
              <textarea
                id="category-description"
                className="input textarea"
                rows={2}
                value={categoryForm.description}
                onChange={(event) =>
                  setCategoryForm((prev) => ({
                    ...prev,
                    description: event.target.value,
                  }))
                }
                placeholder="Short summary"
              />
            </div>
          </div>
          <div className="form-actions">
            <button className="button primary" type="submit">
              {editingId ? "Update category" : "Add category"}
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
              placeholder="Search categories"
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
        {categoriesQuery.isLoading && (
          <div className="state">Loading categories…</div>
        )}
        {categoriesQuery.isError && !error && (
          <div className="state error">
            {getApiErrorMessage(
              categoriesQuery.error,
              "Failed to load categories. Please try again."
            )}
          </div>
        )}

        <div className="table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Description</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {categories.length === 0 && !categoriesQuery.isLoading && (
                <tr>
                  <td colSpan={4} className="table-empty">
                    No categories yet.
                  </td>
                </tr>
              )}
              {categories.map((category) => (
                <tr key={category.id}>
                  <td>{category.id}</td>
                  <td>{category.name}</td>
                  <td className="table-muted">{category.description || "-"}</td>
                  <td>
                    <div className="table-actions">
                      <button
                        className="button ghost"
                        type="button"
                        onClick={() => handleEdit(category)}
                      >
                        Edit
                      </button>
                      <button
                        className="button ghost"
                        type="button"
                        onClick={() => setPendingDelete(category)}
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
        title="Delete category?"
        description={
          pendingDelete
            ? `This will remove "${pendingDelete.name}" from the catalog.`
            : ""
        }
        confirmLabel="Delete category"
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
