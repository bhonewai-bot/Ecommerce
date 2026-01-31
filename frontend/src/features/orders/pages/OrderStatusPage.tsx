import { useMemo } from "react";
import { Link, useParams } from "react-router-dom";
import { useOrder } from "../queries";
import type { Order } from "../../../shared/types/api";

function formatMoney(value: number, currency: string) {
  return `${currency} ${value.toFixed(2)}`;
}

function OrderSummary({ order }: { order: Order }) {
  return (
    <div className="order-summary">
      <div>
        <span className="label">Status</span>
        <strong>{order.status}</strong>
      </div>
      <div>
        <span className="label">Subtotal</span>
        <strong>{formatMoney(order.subtotalAmount, order.currency)}</strong>
      </div>
      <div>
        <span className="label">Discount</span>
        <strong>-{formatMoney(order.discountAmount, order.currency)}</strong>
      </div>
      <div>
        <span className="label">Tax</span>
        <strong>{formatMoney(order.taxAmount, order.currency)}</strong>
      </div>
      <div className="order-total">
        <span className="label">Total</span>
        <strong>{formatMoney(order.totalAmount, order.currency)}</strong>
      </div>
    </div>
  );
}

export default function OrderStatusPage() {
  const { publicId } = useParams();
  const orderQuery = useOrder(publicId);
  const hasStripeKey = Boolean(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

  const statusMessage = useMemo(() => {
    const status = orderQuery.data?.status;
    if (status === "PendingPayment") {
      return "Waiting for payment...";
    }
    if (status === "Paid") {
      return "Payment received. Your order is being prepared.";
    }
    if (status === "Fulfilled") {
      return "Order fulfilled. Thank you for shopping!";
    }
    if (status === "Cancelled") {
      return "Order was cancelled.";
    }
    return "";
  }, [orderQuery.data?.status]);

  if (!publicId) {
    return <div className="state error">Missing order ID.</div>;
  }

  if (orderQuery.isLoading) {
    return <div className="state">Loading order...</div>;
  }

  if (orderQuery.isError) {
    const message =
      orderQuery.error instanceof Error
        ? orderQuery.error.message
        : "Failed to load order.";
    return <div className="state error">{message}</div>;
  }

  if (!orderQuery.data) {
    return <div className="state">Order not found.</div>;
  }

  const order = orderQuery.data;
  const showPayNow = order.status === "PendingPayment";

  return (
    <section className="order-page">
      <div className="page-heading">
        <h1>Order {order.publicId}</h1>
        <p className="subtitle">Track payment and fulfillment status.</p>
      </div>
      {statusMessage && <div className="state">{statusMessage}</div>}
      {showPayNow && (
        <div className="checkout-actions">
          {hasStripeKey ? (
            <Link className="button primary" to={`/pay/${order.publicId}`}>
              Pay now
            </Link>
          ) : (
            <>
              <button className="button primary" type="button" disabled>
                Pay now
              </button>
              <div className="state error">
                Stripe publishable key is missing. Set VITE_STRIPE_PUBLISHABLE_KEY.
              </div>
            </>
          )}
        </div>
      )}
      <OrderSummary order={order} />
      <div className="order-items">
        <h2>Items</h2>
        <ul>
          {order.items.map((item) => (
            <li key={`${item.productId}-${item.productName}`}>
              <div>
                <strong>{item.productName}</strong>
                <span className="note">
                  {item.quantity} × {formatMoney(item.unitPrice, order.currency)}
                </span>
              </div>
              <span>{formatMoney(item.lineTotal, order.currency)}</span>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
