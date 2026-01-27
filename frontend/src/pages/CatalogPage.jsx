import { useEffect, useMemo, useState } from "react";
import { api } from "../api/client";
import ProductCard from "../components/ProductCard";

export default function CatalogPage() {
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");

  useEffect(() => {
    let isMounted = true;
    setStatus("loading");

    Promise.all([api.getCategories(), api.getProducts()])
      .then(([categoriesData, productsData]) => {
        if (!isMounted) return;
        setCategories(categoriesData);
        setProducts(productsData);
        setStatus("ready");
      })
      .catch((err) => {
        if (!isMounted) return;
        setError(err.message || "Failed to load catalog.");
        setStatus("error");
      });

    return () => {
      isMounted = false;
    };
  }, []);

  const categoryMap = useMemo(() => {
    return Object.fromEntries(categories.map((cat) => [cat.id, cat.name]));
  }, [categories]);

  const filteredProducts = useMemo(() => {
    if (selectedCategory === "all") return products;
    return products.filter((product) => product.categoryId === selectedCategory);
  }, [products, selectedCategory]);

  return (
    <section className="catalog-page">
      <div className="hero">
        <div>
          <p className="eyebrow">Today’s deals</p>
          <h1>Find the best picks, fast.</h1>
          <p className="subtitle">
            Browse curated categories and snag everyday essentials with
            confidence.
          </p>
          <div className="hero-actions">
            <button className="button primary" type="button">
              Shop deals
            </button>
            <button className="button ghost" type="button">
              Explore categories
            </button>
          </div>
        </div>
        {/*<div className="hero-card">
          <p>Categories live</p>
          <strong>{categories.length}</strong>
          <span>and counting</span>
        </div>*/}
      </div>

      <div className="filters">
        <button
          className={selectedCategory === "all" ? "chip active" : "chip"}
          type="button"
          onClick={() => setSelectedCategory("all")}
        >
          All
        </button>
        {categories.map((category) => (
          <button
            key={category.id}
            className={selectedCategory === category.id ? "chip active" : "chip"}
            type="button"
            onClick={() => setSelectedCategory(category.id)}
          >
            {category.name}
          </button>
        ))}
      </div>

      {status === "loading" && <div className="state">Loading catalog…</div>}
      {status === "error" && <div className="state error">{error}</div>}

      {status === "ready" && (
        <div className="grid">
          {filteredProducts.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              category={categoryMap[product.categoryId]}
            />
          ))}
        </div>
      )}
    </section>
  );
}
