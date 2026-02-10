import { Link, NavLink, useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { useCart } from "../../features/cart/useCart";

const navClassName = ({ isActive }: { isActive: boolean }) =>
  isActive ? "nav-link nav-link--active" : "nav-link";

export default function SiteHeader() {
  const [searchParams, setSearchParams] = useSearchParams();
  const queryParam = searchParams.get("q") ?? "";
  const categoryParam = searchParams.get("category");
  const [query, setQuery] = useState("");
  const { count } = useCart();

  useEffect(() => {
    setQuery(queryParam);
  }, [queryParam]);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const params = new URLSearchParams();
    if (query.trim()) {
      params.set("q", query.trim());
    }
    if (categoryParam) {
      params.set("category", categoryParam);
    }
    setSearchParams(params);
  };

  return (
    <header className="site-header">
      <div className="header-inner">
        <Link className="brand" to="/">
          <span className="brand-letter blue">e</span>
          <span className="brand-letter red">c</span>
          <span className="brand-letter yellow">o</span>
          <span className="brand-letter green">m</span>
        </Link>
        <form className="search-bar" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Search for anything"
            aria-label="Search products"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
          />
          <button className="button primary" type="submit">
            Search
          </button>
        </form>
        <nav className="nav">
          <NavLink to="/" className={navClassName}>
            Home
          </NavLink>
          <NavLink to="/cart" className={navClassName}>
            Cart {count > 0 ? `(${count})` : ""}
          </NavLink>
          <span className="nav-pill">User storefront</span>
        </nav>
      </div>
    </header>
  );
}
