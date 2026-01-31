import { NavLink } from "react-router-dom";
import type { ReactElement } from "react";

const navClassName = ({ isActive }: { isActive: boolean }) =>
  isActive ? "sidebar-link sidebar-link--active" : "sidebar-link";

const IconDashboard = (): ReactElement => (
  <svg className="sidebar-icon" viewBox="0 0 24 24" aria-hidden="true">
    <rect x="3" y="3" width="8" height="8" rx="2" />
    <rect x="13" y="3" width="8" height="5" rx="2" />
    <rect x="13" y="10" width="8" height="11" rx="2" />
    <rect x="3" y="13" width="8" height="8" rx="2" />
  </svg>
);

const IconBox = (): ReactElement => (
  <svg className="sidebar-icon" viewBox="0 0 24 24" aria-hidden="true">
    <path d="M3 7.5 12 3l9 4.5-9 4.5-9-4.5Z" />
    <path d="M3 7.5V16l9 5 9-5V7.5" />
    <path d="M12 12v9" />
  </svg>
);

const IconTags = (): ReactElement => (
  <svg className="sidebar-icon" viewBox="0 0 24 24" aria-hidden="true">
    <path d="M3 12.5V6a3 3 0 0 1 3-3h6.5L21 11.5l-7.5 7.5L3 12.5Z" />
    <circle cx="9" cy="8" r="1.5" />
  </svg>
);

type AdminSidebarProps = {
  isOpen: boolean;
  onClose: () => void;
};

export default function AdminSidebar({ isOpen, onClose }: AdminSidebarProps) {
  return (
    <aside className={`admin-sidebar${isOpen ? " is-open" : ""}`}>
      <div className="sidebar-mobile">
        <span>Admin menu</span>
        <button className="sidebar-close" type="button" onClick={onClose}>
          Close
        </button>
      </div>
      <div className="sidebar-brand">
        <span className="brand-letter blue">a</span>
        <span className="brand-letter red">d</span>
        <span className="brand-letter yellow">m</span>
        <span className="brand-letter green">i</span>
        <span className="brand-letter blue">n</span>
      </div>
      <nav className="sidebar-nav">
        <NavLink to="/admin" end className={navClassName}>
          <IconDashboard />
          Dashboard
        </NavLink>
        <NavLink to="/admin/products" className={navClassName}>
          <IconBox />
          Products
        </NavLink>
        <NavLink to="/admin/categories" className={navClassName}>
          <IconTags />
          Categories
        </NavLink>
      </nav>
      <div className="sidebar-footer">
        <NavLink to="/" className="sidebar-link">
          <svg className="sidebar-icon" viewBox="0 0 24 24" aria-hidden="true">
            <path d="M10 19H5v-7l7-6 7 6v7h-5" />
            <path d="M10 19v-5h4v5" />
          </svg>
          Back to storefront
        </NavLink>
      </div>
    </aside>
  );
}
