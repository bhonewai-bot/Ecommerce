import { Link } from "react-router-dom";
import type { Product } from "../../types/api";

type ProductCardProps = {
  product: Product;
  category?: string;
};

export default function ProductCard({ product, category }: ProductCardProps) {
  return (
    <article className="product-card">
      <div className="product-media">
        <div className="product-tag">{category || "Uncategorized"}</div>
        <div className="product-placeholder" aria-hidden="true" />
      </div>
      <div className="product-body">
        <h3>{product.name}</h3>
        <p>{product.description || "No description yet."}</p>
        <span className="product-delivery">Free delivery · 2-day</span>
        <div className="product-footer">
          <span className="price">${product.price.toFixed(2)}</span>
          <Link className="button ghost" to={`/products/${product.id}`}>
            View
          </Link>
        </div>
      </div>
    </article>
  );
}
