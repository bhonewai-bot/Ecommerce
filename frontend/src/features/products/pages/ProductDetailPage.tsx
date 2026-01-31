import { Link, useNavigate, useParams } from "react-router-dom";
import type { Product } from "../../../shared/types/api";
import { useProduct } from "../queries";
import { addToCart } from "../../cart/store";

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

  if (productQuery.isLoading) {
    return <div className="state">Loading product…</div>;
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
            <span className="price">${product.price.toFixed(2)}</span>
            <span className="sku">Item #{product.id}</span>
          </div>
          <div className="detail-actions">
            <button className="button primary" type="button" onClick={handleAddToCart}>
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
