import { Link, NavLink } from "react-router-dom";

const navClassName = ({ isActive }) =>
  isActive ? "nav-link nav-link--active" : "nav-link";

export default function SiteHeader() {
  return (
    <header className="site-header">
      <div className="header-inner">
        <Link className="brand" to="/">
          <span className="brand-letter blue">e</span>
          <span className="brand-letter red">c</span>
          <span className="brand-letter yellow">o</span>
          <span className="brand-letter green">m</span>
        </Link>
        <div className="search-bar">
          <input
            type="text"
            placeholder="Search for anything"
            aria-label="Search products"
          />
          <button className="button primary" type="button">
            Search
          </button>
        </div>
        <nav className="nav">
          <NavLink to="/" className={navClassName}>
            Home
          </NavLink>
          <span className="nav-pill">User storefront</span>
        </nav>
      </div>
    </header>
  );
}
