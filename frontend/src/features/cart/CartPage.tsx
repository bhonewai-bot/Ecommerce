import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCheckout } from "../checkout/queries";
import { clearCart, removeItem, updateQuantity } from "./store";
import { useCart } from "./useCart";

function formatMoney(value: number) {
  return `$${value.toFixed(2)}`;
}

export default function CartPage() {
  const navigate = useNavigate();
  const { items, total, count } = useCart();
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const checkoutMutation = useCheckout();

  const summaryLabel = useMemo(() => {
    if (!count) return "Cart is empty";
    return `${count} item${count === 1 ? "" : "s"}`;
  }, [count]);

  const handleCheckout = () => {
    if (!items.length) return;
    setError("");
    checkoutMutation.mutate(
      {
        customerEmail: email.trim() ? email.trim() : undefined,
        items: items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
      },
      {
        onSuccess: (response) => {
          clearCart();
          navigate(`/pay/${response.publicId}`);
        },
        onError: (err) => {
          setError(err instanceof Error ? err.message : "Checkout failed.");
        },
      }
    );
  };

  return (
    <section className="cart-page">
      <div className="page-heading">
        <h1>Shopping cart</h1>
        <p className="subtitle">Review your picks before checkout.</p>
      </div>

      <div className="cart-grid">
        <div className="cart-list">
          <div className="cart-header">
            <h2>{summaryLabel}</h2>
          </div>
          {!items.length && <div className="state">Your cart is empty.</div>}
          {items.map((item) => (
            <div className="cart-item" key={item.productId}>
              <div className="cart-item-info">
                <div className="cart-item-media" />
                <div>
                  <strong>{item.name}</strong>
                  <span className="note">{formatMoney(item.price)} each</span>
                </div>
              </div>
              <div className="cart-item-actions">
                <div className="qty-control">
                  <button
                    className="button ghost"
                    type="button"
                    onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                  >
                    −
                  </button>
                  <span>{item.quantity}</span>
                  <button
                    className="button ghost"
                    type="button"
                    onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                  >
                    +
                  </button>
                </div>
                <span className="cart-price">
                  {formatMoney(item.price * item.quantity)}
                </span>
                <button
                  className="button ghost"
                  type="button"
                  onClick={() => removeItem(item.productId)}
                >
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>

        <aside className="cart-summary">
          <h3>Order summary</h3>
          <div className="cart-summary-row">
            <span>Items</span>
            <strong>{summaryLabel}</strong>
          </div>
          <div className="cart-summary-row">
            <span>Estimated total</span>
            <strong>{formatMoney(total)}</strong>
          </div>
          <label className="form-row">
            <span>Email (optional)</span>
            <input
              className="input"
              type="email"
              placeholder="you@example.com"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>
          {error && <div className="state error">{error}</div>}
          <button
            className="button primary"
            type="button"
            onClick={handleCheckout}
            disabled={!items.length || checkoutMutation.isPending}
          >
            {checkoutMutation.isPending ? "Processing..." : "Checkout"}
          </button>
        </aside>
      </div>
    </section>
  );
}
