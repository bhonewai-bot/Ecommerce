import { Link } from "react-router-dom";
import type { Product } from "../../../shared/types/api";
import { useCategories } from "../../categories/queries";
import { useProducts } from "../../products/queries";
import { getApiErrorMessage } from "../../../shared/utils/errorMessages";

export default function DashboardPage() {
  const categoriesQuery = useCategories({ page: 1, pageSize: 1 });
  const productsQuery = useProducts({
    page: 1,
    pageSize: 5,
    sortBy: "id",
    sortOrder: "desc",
  });

  const stats = {
    categories: categoriesQuery.data?.totalCount ?? 0,
    products: productsQuery.data?.totalCount ?? 0,
  };

  const recentProducts: Product[] = productsQuery.data?.items ?? [];
  const isLoading = categoriesQuery.isLoading || productsQuery.isLoading;
  const error = categoriesQuery.isError
    ? getApiErrorMessage(
        categoriesQuery.error,
        "Failed to load dashboard. Please try again."
      )
    : productsQuery.isError
      ? getApiErrorMessage(
          productsQuery.error,
          "Failed to load dashboard. Please try again."
        )
      : "";

  return (
    <section className="admin-page">
      <div className="admin-hero">
        <div>
          <p className="eyebrow">Admin dashboard</p>
          <h1>Store overview</h1>
          <p className="subtitle">
            Track catalog health, then jump into products or categories.
          </p>
          <div className="admin-actions">
            <Link className="button primary" to="/admin/products">
              Manage products
            </Link>
            <Link className="button ghost" to="/admin/categories">
              Manage categories
            </Link>
          </div>
        </div>
        <div className="admin-card">
          <strong>{stats.products}</strong>
          <span>products in catalog</span>
          <span className="admin-subtle">{stats.categories} categories</span>
        </div>
      </div>

      {isLoading && <div className="state">Loading dashboard…</div>}
      {error && <div className="state error">{error}</div>}

      {!isLoading && !error && (
        <div className="admin-panel">
          <div className="admin-panel-header">
            <h3>Recently added products</h3>
            <Link className="button ghost" to="/admin/products">
              View all
            </Link>
          </div>
          <div className="table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Price</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {recentProducts.length === 0 && (
                  <tr>
                    <td colSpan={4} className="table-empty">
                      No products yet.
                    </td>
                  </tr>
                )}
                {recentProducts.map((product) => (
                  <tr key={product.id}>
                    <td>{product.id}</td>
                    <td>{product.name}</td>
                    <td>${Number(product.price || 0).toFixed(2)}</td>
                    <td>
                      <Link className="button ghost" to="/admin/products">
                        Edit
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  );
}
