import { Link } from "react-router-dom";

export default function SiteFooter() {
  const year = new Date().getFullYear();

  return (
    <footer className="site-footer" aria-label="Site footer">
      <div className="site-footer-inner">
        <section className="site-footer-brand">
          <h4>Evercart</h4>
          <p className="site-footer-copy">
            Shop confidently with secure checkout, trusted payments, and easy
            order tracking.
          </p>
        </section>

        <nav className="site-footer-links" aria-label="Help links">
          <h4>Help</h4>
          <Link to="/cart">Checkout help</Link>
          <Link to="/cart">Shipping & returns</Link>
          <Link to="/cart">Contact support</Link>
        </nav>

        <nav className="site-footer-links" aria-label="Legal links">
          <h4>Legal</h4>
          <Link to="/">Privacy</Link>
          <Link to="/">Terms</Link>
          <Link to="/">Cookies</Link>
        </nav>
      </div>
      <div className="site-footer-bottom">
        <span>{`© ${year} Evercart. All rights reserved.`}</span>
        <span>Powered by Stripe for secure payments.</span>
      </div>
    </footer>
  );
}
