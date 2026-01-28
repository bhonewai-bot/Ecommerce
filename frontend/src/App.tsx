import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import StorefrontLayout from "./storefront/StorefrontLayout";
import CatalogPage from "./storefront/pages/CatalogPage";
import ProductDetailPage from "./storefront/pages/ProductDetailPage";
import AdminLayout from "./admin/AdminLayout";
import DashboardPage from "./admin/pages/DashboardPage";
import ProductsPage from "./admin/pages/ProductsPage";
import CategoriesPage from "./admin/pages/CategoriesPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<StorefrontLayout />}>
          <Route path="/" element={<CatalogPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
        </Route>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="categories" element={<CategoriesPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
