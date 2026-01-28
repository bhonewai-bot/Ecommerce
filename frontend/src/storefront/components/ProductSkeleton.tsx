export default function ProductSkeleton() {
  return (
    <article className="product-card skeleton-card" aria-hidden="true">
      <div className="product-media skeleton-block" />
      <div className="product-body">
        <div className="skeleton-line wide" />
        <div className="skeleton-line" />
        <div className="skeleton-line short" />
        <div className="skeleton-line price" />
      </div>
    </article>
  );
}
