import { BrowserRouter, Route, Routes } from "react-router-dom";
import "../shared/styles/App.css";
import StorefrontLayout from "../layouts/StorefrontLayout";
import CatalogPage from "../features/products/pages/CatalogPage";
import ProductDetailPage from "../features/products/pages/ProductDetailPage";
import PayPage from "../features/payments/pages/PayPage";
import OrderStatusPage from "../features/orders/pages/OrderStatusPage";
import CartPage from "../features/cart/CartPage";
import CheckoutSuccessPage from "../features/checkout/pages/CheckoutSuccessPage";
import CheckoutCancelPage from "../features/checkout/pages/CheckoutCancelPage";
import AdminLayout from "../layouts/AdminLayout";
import DashboardPage from "../features/admin/pages/DashboardPage";
import ProductsPage from "../features/admin/pages/ProductsPage";
import CategoriesPage from "../features/admin/pages/CategoriesPage";
import OrdersPage from "../features/admin/pages/OrdersPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<StorefrontLayout />}>
          <Route path="/" element={<CatalogPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/checkout/success" element={<CheckoutSuccessPage />} />
          <Route path="/checkout/cancel" element={<CheckoutCancelPage />} />
          <Route path="/pay/:publicId" element={<PayPage />} />
          <Route path="/orders/:publicId" element={<OrderStatusPage />} />
        </Route>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="categories" element={<CategoriesPage />} />
          <Route path="orders" element={<OrdersPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
