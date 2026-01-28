import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { api } from "../../api/client";
import ProductCard from "../components/ProductCard";
import ProductSkeleton from "../components/ProductSkeleton";

export default function CatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const queryParam = searchParams.get("q") ?? "";
  const categoryParam = searchParams.get("category") ?? "all";
  const pageParam = Number(searchParams.get("page") ?? "1") || 1;
  const sortByParam = searchParams.get("sortBy") ?? "id";
  const sortOrderParam = searchParams.get("sortOrder") ?? "asc";
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  const [pageInfo, setPageInfo] = useState({ page: 1, pageSize: 20, total: 0 });
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [status, setStatus] = useState("idle");
  const [error, setError] = useState("");

  useEffect(() => {
    let isMounted = true;
    setStatus("loading");

    const categoryId =
      categoryParam === "all" ? null : Number(categoryParam);

    Promise.all([
      api.getCategories(),
      api.getProducts(pageParam, 10, sortByParam, sortOrderParam, categoryId),
    ])
      .then(([categoriesData, productsData]) => {
        if (!isMounted) return;
        setCategories(categoriesData);
        setProducts(productsData.items);
        setPageInfo({
          page: productsData.page,
          pageSize: productsData.pageSize,
          total: productsData.totalCount,
        });
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
  }, [pageParam, sortByParam, sortOrderParam, categoryParam]);

  useEffect(() => {
    setSelectedCategory(categoryParam === "all" ? "all" : Number(categoryParam));
  }, [categoryParam, queryParam]);

  const categoryMap = useMemo(() => {
    return Object.fromEntries(categories.map((cat) => [cat.id, cat.name]));
  }, [categories]);

  const filteredProducts = useMemo(() => {
    const normalizedQuery = queryParam.trim().toLowerCase();
    return products.filter((product) => {
      const matchesQuery =
        !normalizedQuery ||
        product.name.toLowerCase().includes(normalizedQuery) ||
        (product.description || "").toLowerCase().includes(normalizedQuery);
      return matchesQuery;
    });
  }, [products, queryParam]);

  const handleCategoryChange = (value) => {
    const params = new URLSearchParams(searchParams);
    if (value === "all") {
      params.delete("category");
    } else {
      params.set("category", value.toString());
    }
    if (queryParam.trim()) {
      params.set("q", queryParam.trim());
    }
    params.delete("page");
    setSearchParams(params);
  };

  const totalPages = Math.max(1, Math.ceil(pageInfo.total / pageInfo.pageSize));
  const currentPage = Math.min(Math.max(pageParam, 1), totalPages);

  const handlePageChange = (nextPage) => {
    const params = new URLSearchParams(searchParams);
    if (nextPage <= 1) {
      params.delete("page");
    } else {
      params.set("page", nextPage.toString());
    }
    setSearchParams(params);
  };

  const handleSortChange = (event) => {
    const value = event.target.value;
    const [nextSortBy, nextSortOrder] = value.split(":");
    const params = new URLSearchParams(searchParams);
    params.set("sortBy", nextSortBy);
    params.set("sortOrder", nextSortOrder);
    params.delete("page");
    setSearchParams(params);
  };

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

      <div className="catalog-toolbar">
        <div className="catalog-meta">
          {filteredProducts.length} items
          {queryParam && (
            <span className="catalog-query">
              {" "}
              · Results for "{queryParam}"
              <button
                className="clear-link"
                type="button"
                onClick={() => {
                  const params = new URLSearchParams(searchParams);
                  params.delete("q");
                  setSearchParams(params);
                }}
              >
                Clear
              </button>
            </span>
          )}
        </div>
        <label className="sort-control">
          Sort by
          <select
            value={`${sortByParam}:${sortOrderParam}`}
            onChange={handleSortChange}
          >
            <option value="id:asc">Newest</option>
            <option value="name:asc">Name (A–Z)</option>
            <option value="name:desc">Name (Z–A)</option>
            <option value="price:asc">Price (Low to High)</option>
            <option value="price:desc">Price (High to Low)</option>
          </select>
        </label>
        <div className="catalog-meta">
          Page {currentPage} of {totalPages}
        </div>
      </div>

      <div className="filters">
        <button
          className={selectedCategory === "all" ? "chip active" : "chip"}
          type="button"
          onClick={() => handleCategoryChange("all")}
        >
          All
        </button>
        {categories.map((category) => (
          <button
            key={category.id}
            className={selectedCategory === category.id ? "chip active" : "chip"}
            type="button"
            onClick={() => handleCategoryChange(category.id)}
          >
            {category.name}
          </button>
        ))}
      </div>

      {status === "loading" && (
        <div className="grid">
          {Array.from({ length: 8 }).map((_, index) => (
            <ProductSkeleton key={`skeleton-${index}`} />
          ))}
        </div>
      )}
      {status === "error" && <div className="state error">{error}</div>}

      {status === "ready" && (
        <>
          <div className="grid">
            {filteredProducts.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                category={categoryMap[product.categoryId]}
              />
            ))}
            {filteredProducts.length === 0 && (
              <div className="state">No products match your filters.</div>
            )}
          </div>
          <div className="pagination">
            <button
              className="button ghost"
              type="button"
              onClick={() => handlePageChange(currentPage - 1)}
              disabled={currentPage <= 1}
            >
              Previous
            </button>
            <span>
              Page {currentPage} of {totalPages}
            </span>
            <button
              className="button ghost"
              type="button"
              onClick={() => handlePageChange(currentPage + 1)}
              disabled={currentPage >= totalPages}
            >
              Next
            </button>
          </div>
        </>
      )}
    </section>
  );
}
