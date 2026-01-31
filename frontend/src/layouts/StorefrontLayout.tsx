import { Outlet } from "react-router-dom";
import SiteHeader from "../shared/components/SiteHeader";

export default function StorefrontLayout() {
  return (
    <div className="app-shell storefront-shell">
      <SiteHeader />
      <main className="main">
        <Outlet />
      </main>
      <footer className="footer">
        <span>Evercart MVP · Built for portfolio showcase</span>
        <span>Admin dashboard available at /admin</span>
      </footer>
    </div>
  );
}
