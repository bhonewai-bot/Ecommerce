import { Outlet, useLocation } from "react-router-dom";
import SiteHeader from "../shared/components/SiteHeader";
import SiteFooter from "../shared/components/SiteFooter";

export default function StorefrontLayout() {
  const location = useLocation();
  const isPaymentPage = location.pathname === "/pay" || location.pathname.startsWith("/pay/");

  return (
    <div className="app-shell storefront-shell">
      <SiteHeader />
      <main className="main">
        <Outlet />
      </main>
      {!isPaymentPage && <SiteFooter />}
    </div>
  );
}
