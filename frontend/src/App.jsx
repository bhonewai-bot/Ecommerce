import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import SiteHeader from "./components/SiteHeader";
import CatalogPage from "./pages/CatalogPage";
import ProductDetailPage from "./pages/ProductDetailPage";

export default function App() {
  return (
    <BrowserRouter>
      <div className="app-shell">
        <SiteHeader />
        <main className="main">
          <Routes>
            <Route path="/" element={<CatalogPage />} />
            <Route path="/products/:id" element={<ProductDetailPage />} />
          </Routes>
        </main>
        <footer className="footer">
          <span>Evercart MVP · Built for portfolio showcase</span>
          <span>Admin dashboard arrives next.</span>
        </footer>
      </div>
    </BrowserRouter>
  );
}
