import { useEffect } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import type { Product } from "../../../shared/types/api";
import { useProduct } from "../queries";
import { addToCart } from "../../cart/store";

const thbFormatter = new Intl.NumberFormat("th-TH", {
  style: "currency",
  currency: "THB",
  maximumFractionDigits: 2,
});

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = id ? Number(id) : NaN;
  const productQuery = useProduct({ id: numericId });
  const navigate = useNavigate();

  const product: Product | undefined = productQuery.data;
  const errorMessage =
      productQuery.error instanceof Error
          ? productQuery.error.message
          : "Failed to load product.";

  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: "auto" });
  }, [id]);

  if (productQuery.isLoading) {
    return (
      <section
        className="product-detail product-detail--loading"
        aria-busy="true"
        aria-live="polite"
      >
        <div className="back-link skeleton-line short" aria-hidden="true" />
        <div className="detail-card detail-card--skeleton">
          <div className="detail-media skeleton-block" aria-hidden="true" />
          <div className="detail-body">
            <div className="skeleton-line detail-title" aria-hidden="true" />
            <div className="skeleton-line wide" aria-hidden="true" />
            <div className="skeleton-line" aria-hidden="true" />
            <div className="skeleton-line short" aria-hidden="true" />
            <div className="detail-meta">
              <span className="skeleton-line price" aria-hidden="true" />
              <span className="skeleton-line short" aria-hidden="true" />
            </div>
            <div className="detail-actions">
              <span className="skeleton-block skeleton-pill" aria-hidden="true" />
              <span
                className="skeleton-block skeleton-pill skeleton-pill--ghost"
                aria-hidden="true"
              />
            </div>
          </div>
        </div>
      </section>
    );
  }

  if (!id || Number.isNaN(numericId) || productQuery.isError) {
    return (
        <div className="state error">
          {id ? errorMessage : "Product not found."}
          <Link className="button ghost" to="/">
            Back to catalog
          </Link>
        </div>
    );
  }

  if (!product) {
    return null;
  }

  const handleAddToCart = () => {
    addToCart(
        {
          productId: product.id,
          name: product.name,
          price: product.price,
        },
        1
    );
    navigate("/cart");
  };

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
              <span className="price">{thbFormatter.format(product.price)}</span>
              <span className="sku">• In stock</span>
            </div>

            <div className="detail-actions">
              <button className="button primary" type="button" onClick={handleAddToCart}>
                Add to cart
              </button>
              <button className="button ghost" type="button">
                Buy now
              </button>
            </div>
            <div className="detail-shipping">
              <span className={"text-sm font-semibold mr-10"}>Shipping:</span>
              <span className="product-delivery">Free 2-4 day delivery</span>
            </div>
            {/*<p className="note">
              Checkout flow will be added after MVP validation.
            </p>*/}
          </div>
        </div>
      </section>
  );
}
