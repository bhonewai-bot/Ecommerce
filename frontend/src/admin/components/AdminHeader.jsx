import { Link, NavLink } from "react-router-dom";

const navClassName = ({ isActive }) =>
  isActive ? "nav-link nav-link--active" : "nav-link";

export default function AdminHeader() {
  return (
    <header className="admin-header">
      <div className="header-inner">
        <Link className="brand" to="/admin">
          <span className="brand-letter blue">a</span>
          <span className="brand-letter red">d</span>
          <span className="brand-letter yellow">m</span>
          <span className="brand-letter green">i</span>
          <span className="brand-letter blue">n</span>
        </Link>
        <nav className="nav">
          <NavLink to="/admin" end className={navClassName}>
            Dashboard
          </NavLink>
          <NavLink to="/admin/products" className={navClassName}>
            Products
          </NavLink>
          <NavLink to="/admin/categories" className={navClassName}>
            Categories
          </NavLink>
          <NavLink to="/" className={navClassName}>
            Storefront
          </NavLink>
          <span className="nav-pill">Admin tools</span>
        </nav>
      </div>
    </header>
  );
}
