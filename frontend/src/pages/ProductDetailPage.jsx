import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../api/client";

export default function ProductDetailPage() {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");

  useEffect(() => {
    let isMounted = true;
    setStatus("loading");

    api
      .getProduct(id)
      .then((data) => {
        if (!isMounted) return;
        setProduct(data);
        setStatus("ready");
      })
      .catch((err) => {
        if (!isMounted) return;
        setError(err.message || "Failed to load product.");
        setStatus("error");
      });

    return () => {
      isMounted = false;
    };
  }, [id]);

  if (status === "loading") {
    return <div className="state">Loading product…</div>;
  }

  if (status === "error") {
    return (
      <div className="state error">
        {error}
        <Link className="button ghost" to="/">
          Back to catalog
        </Link>
      </div>
    );
  }

  if (!product) {
    return null;
  }

  return (
    <section className="product-detail">
      <Link className="back-link" to="/">
        ← Back to catalog
      </Link>
      <div className="detail-card">
        <div className="detail-media" />
        <div className="detail-body">
          <h1>{product.name}</h1>
          <p className="detail-description">
            {product.description || "No description for this product yet."}
          </p>
          <div className="detail-meta">
            <span className="price">${product.price.toFixed(2)}</span>
            <span className="sku">Item #{product.id}</span>
          </div>
          <div className="detail-actions">
            <button className="button primary" type="button">
              Add to cart
            </button>
            <button className="button ghost" type="button">
              Save for later
            </button>
          </div>
          <p className="note">
            Checkout flow will be added after MVP validation.
          </p>
        </div>
      </div>
    </section>
  );
}
