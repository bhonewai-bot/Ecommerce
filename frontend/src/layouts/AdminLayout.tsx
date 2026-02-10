import { Outlet } from "react-router-dom";
import { useState } from "react";
import AdminSidebar from "../features/admin/components/AdminSidebar";

export default function AdminLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="app-shell admin-shell">
      <AdminSidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <div className="admin-main">
        <header className="admin-topbar">
          <div className="admin-topbar-title">
            <p className="eyebrow">Admin workspace</p>
            <h2>Catalog control center</h2>
          </div>
          <button
            className="sidebar-toggle"
            type="button"
            onClick={() => setSidebarOpen((prev) => !prev)}
            aria-expanded={sidebarOpen}
            aria-label="Toggle sidebar"
          >
            <span />
            <span />
            <span />
          </button>
        </header>
        <main className="main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
