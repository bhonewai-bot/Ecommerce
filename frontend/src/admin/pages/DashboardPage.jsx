import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api/client";

export default function DashboardPage() {
  const [stats, setStats] = useState({ products: 0, categories: 0 });
  const [recentProducts, setRecentProducts] = useState([]);
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");

  useEffect(() => {
    let isMounted = true;
    setStatus("loading");
    setError("");

    Promise.all([
      api.getAdminCategories(1, 1, ""),
      api.getAdminProducts(1, 5, "id", "desc", null, ""),
    ])
      .then(([categoriesData, productsData]) => {
        if (!isMounted) return;
        setStats({
          categories: categoriesData.totalCount ?? categoriesData.items?.length ?? 0,
          products: productsData.totalCount ?? productsData.items.length,
        });
        setRecentProducts(productsData.items || []);
        setStatus("ready");
      })
      .catch((err) => {
        if (!isMounted) return;
        setError(err.message || "Failed to load dashboard.");
        setStatus("error");
      });

    return () => {
      isMounted = false;
    };
  }, []);

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

      {status === "loading" && <div className="state">Loading dashboard…</div>}
      {status === "error" && <div className="state error">{error}</div>}

      {status === "ready" && (
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
