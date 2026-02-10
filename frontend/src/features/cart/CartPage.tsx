import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCheckout } from "../checkout/queries";
import {
  clearCart,
  clearPendingOrder,
  removeItem,
  setPendingOrder,
  updatePendingOrderStatus,
  updateQuantity,
} from "./store";
import { useCart } from "./useCart";
import { usePendingOrder } from "./usePendingOrder";
import { cancelOrder, getOrderByPublicId } from "../orders/api";

function formatMoney(value: number) {
  return `$${value.toFixed(2)}`;
}

export default function CartPage() {
  const navigate = useNavigate();
  const { items, total, count } = useCart();
  const pendingOrder = usePendingOrder();
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [pendingError, setPendingError] = useState("");
  const checkoutMutation = useCheckout();
  const idempotencyKeyRef = useRef<string | null>(null);

  const summaryLabel = useMemo(() => {
    if (!count) return "Cart is empty";
    return `${count} item${count === 1 ? "" : "s"}`;
  }, [count]);

  const handleCheckout = () => {
    if (!items.length) return;
    setError("");
    if (!idempotencyKeyRef.current) {
      idempotencyKeyRef.current = crypto.randomUUID();
    }
    checkoutMutation.mutate(
      {
        input: {
          customerEmail: email.trim() ? email.trim() : undefined,
          items: items.map((item) => ({
            productId: item.productId,
            quantity: item.quantity,
          })),
        },
        idempotencyKey: idempotencyKeyRef.current,
      },
      {
        onSuccess: (response) => {
          setPendingOrder({
            pendingOrderId: response.orderPublicId,
            checkoutSessionId: response.checkoutSessionId ?? null,
            checkoutUrl: response.stripeCheckoutUrl ?? null,
            createdAt: new Date().toISOString(),
            cartSnapshot: items.map((item) => ({ ...item })),
            status: "pending",
          });
          idempotencyKeyRef.current = null;
          if (response.stripeCheckoutUrl) {
            window.location.href = response.stripeCheckoutUrl;
          } else {
            navigate(`/orders/${response.orderPublicId}`);
          }
        },
        onError: (err) => {
          setError(err instanceof Error ? err.message : "Checkout failed.");
          idempotencyKeyRef.current = null;
        },
      }
    );
  };

  useEffect(() => {
    if (!pendingOrder || pendingOrder.status !== "pending") return;
    let isActive = true;
    getOrderByPublicId(pendingOrder.pendingOrderId)
      .then((order) => {
        if (!isActive) return;
        if (order.status === "Paid") {
          clearCart();
          clearPendingOrder();
        }
      })
      .catch(() => {});
    return () => {
      isActive = false;
    };
  }, [pendingOrder?.pendingOrderId, pendingOrder?.status]);

  return (
    <section className="cart-page">
      <div className="page-heading">
        <h1>Shopping cart</h1>
        <p className="subtitle">Review your picks before checkout.</p>
      </div>

      {pendingOrder && (
        <div className="state pending-checkout">
          <div>
            <strong>Pending checkout</strong>
            <div className="note">
              {pendingOrder.status === "pending"
                ? "You have a pending checkout. Continue payment or cancel."
                : `This checkout is ${pendingOrder.status}.`}
            </div>
          </div>
          <div className="pending-actions">
            {pendingOrder.status === "pending" && (
              <>
                <button
                  className="button primary"
                  type="button"
                  onClick={() => {
                    if (pendingOrder.checkoutUrl) {
                      window.location.href = pendingOrder.checkoutUrl;
                      return;
                    }
                    navigate(`/orders/${pendingOrder.pendingOrderId}`);
                  }}
                >
                  Continue payment
                </button>
                <button
                  className="button ghost"
                  type="button"
                  onClick={async () => {
                    setPendingError("");
                    try {
                      await cancelOrder(pendingOrder.pendingOrderId);
                      updatePendingOrderStatus("canceled");
                    } catch (err) {
                      setPendingError(
                        err instanceof Error ? err.message : "Unable to cancel order."
                      );
                    }
                  }}
                >
                  Cancel
                </button>
              </>
            )}
            {pendingOrder.status !== "pending" && (
              <button
                className="button ghost"
                type="button"
                onClick={() => clearPendingOrder()}
              >
                Dismiss
              </button>
            )}
          </div>
        </div>
      )}
      {pendingError && <div className="state error">{pendingError}</div>}

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
                  <span className="note"> • {formatMoney(item.price)} each</span>
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
