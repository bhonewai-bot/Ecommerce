import { useEffect, useRef, useState } from "react";
import { api } from "../../api/client";
import ConfirmDialog from "../components/ConfirmDialog";

const emptyCategoryForm = { name: "", description: "" };

export default function CategoriesPage() {
  const [categories, setCategories] = useState([]);
  const [status, setStatus] = useState("idle");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [categoryForm, setCategoryForm] = useState(emptyCategoryForm);
  const [editingId, setEditingId] = useState(null);
  const [pendingDelete, setPendingDelete] = useState(null);
  const formRef = useRef(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  const loadData = async () => {
    setStatus("loading");
    setError("");
    try {
      const categoriesData = await api.getAdminCategories(page, pageSize, searchQuery);
      setCategories(categoriesData.items ?? categoriesData);
      setTotalCount(categoriesData.totalCount ?? categoriesData.items?.length ?? 0);
      setStatus("ready");
    } catch (err) {
      setError(err.message || "Failed to load categories.");
      setStatus("error");
    }
  };

  useEffect(() => {
    loadData();
  }, [page, pageSize, searchQuery]);

  const resetForm = () => {
    setCategoryForm(emptyCategoryForm);
    setEditingId(null);
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setMessage("");
    setError("");

    const payload = {
      name: categoryForm.name.trim(),
      description: categoryForm.description.trim() || null,
    };

    if (!payload.name) {
      setError("Category name is required.");
      return;
    }

    try {
      if (editingId) {
        await api.updateCategory(editingId, payload);
        setMessage("Category updated.");
      } else {
        await api.createCategory(payload);
        setMessage("Category created.");
      }
      resetForm();
      await loadData();
    } catch (err) {
      setError(err.message || "Failed to save category.");
    }
  };

  const handleEdit = (category) => {
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

  const handleDelete = async (category) => {
    setMessage("");
    setError("");
    try {
      await api.deleteCategory(category.id);
      setMessage("Category deleted.");
      if (editingId === category.id) {
        resetForm();
      }
      await loadData();
    } catch (err) {
      setError(err.message || "Failed to delete category.");
    }
  };

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const currentPage = Math.min(Math.max(page, 1), totalPages);

  return (
    <section className="admin-page">
      <div className="admin-panel">
        <div className="admin-panel-header">
          <h3>Categories</h3>
          <button className="button ghost" type="button" onClick={loadData}>
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
        {status === "loading" && <div className="state">Loading categories…</div>}

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
              {categories.length === 0 && status !== "loading" && (
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
                  <td className="table-muted">
                    {category.description || "-"}
                  </td>
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
