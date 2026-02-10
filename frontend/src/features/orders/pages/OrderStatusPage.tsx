import { useEffect, useMemo } from "react";
import { Link, useLocation, useParams } from "react-router-dom";
import { useOrder } from "../queries";
import type { Order } from "../../../shared/types/api";
import { getApiErrorMessage } from "../../../shared/utils/errorMessages";
import {
  getOrderStatusLabel,
  getOrderStatusMessage,
} from "../../../shared/utils/orderStatus";
import StatusBadge from "../../../shared/components/checkout/StatusBadge";
import InfoBanner from "../../../shared/components/checkout/InfoBanner";
import OrderSummaryCard from "../../../shared/components/checkout/OrderSummaryCard";
import { setLastOrderId } from "../../cart/store";
import { getDisplayOrderId } from "../../../shared/utils/orderId";

function formatMoney(value: number, currency: string) {
  return `${currency} ${value.toFixed(2)}`;
}

function OrderSkeleton() {
  return (
    <section className="order-page" aria-busy="true" aria-live="polite">
      <div className="page-heading">
        <div className="skeleton-line wide" />
        <div className="skeleton-line short" />
      </div>
      <div className="order-summary">
        {Array.from({ length: 5 }).map((_, index) => (
          <div key={`summary-${index}`}>
            <div className="skeleton-line short" />
            <div className="skeleton-line" />
          </div>
        ))}
      </div>
      <div className="order-items">
        <h2>Items</h2>
        <ul>
          {Array.from({ length: 3 }).map((_, index) => (
            <li key={`item-${index}`}>
              <div>
                <div className="skeleton-line wide" />
                <div className="skeleton-line short" />
              </div>
              <div className="skeleton-line short" />
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}

export default function OrderStatusPage() {
  const { publicId } = useParams();
  const location = useLocation();
  const orderQuery = useOrder(publicId);
  const hasStripeKey = Boolean(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);

  const statusMessage = useMemo(() => {
    const status = orderQuery.data?.status;
    return status ? getOrderStatusMessage(status) : "";
  }, [orderQuery.data?.status]);

  useEffect(() => {
    if (!orderQuery.data?.publicId) return;
    setLastOrderId(orderQuery.data.publicId);
  }, [orderQuery.data?.publicId]);

  if (!publicId) {
    return <div className="state error">Missing order ID.</div>;
  }

  if (orderQuery.isLoading) {
    return <OrderSkeleton />;
  }

  if (orderQuery.isError) {
    const message = getApiErrorMessage(
      orderQuery.error,
      "Failed to load order. Please try again."
    );
    return <div className="state error">{message}</div>;
  }

  if (!orderQuery.data) {
    return <div className="state">No orders yet.</div>;
  }

  const order = orderQuery.data;
  const showPayNow = order.status === "PendingPayment" && !order.hasCheckoutSession;
  const showHostedCheckoutNotice =
    order.status === "PendingPayment" && order.hasCheckoutSession;
  const isPaid = order.status === "Paid";
  const paymentSuccess = Boolean(
    (location.state as { paymentSuccess?: boolean } | null)?.paymentSuccess
  );
  const shouldShowStatusMessage = Boolean(statusMessage) &&
    !(paymentSuccess && order.status === "Paid");
  const createdAt = (order as Order & { createdAt?: string }).createdAt;
  const orderDate = createdAt ? new Date(createdAt) : new Date();
  const orderDateText = Number.isNaN(orderDate.getTime())
    ? "Unavailable"
    : orderDate.toLocaleString();

  return (
    <section className="order-page">
      <div className="page-heading">
        <span className="order-kicker">Order tracking</span>
        <h1>Order {getDisplayOrderId(order.publicId)}</h1>
        <p className="subtitle">Track payment and fulfillment status.</p>
      </div>
      {paymentSuccess && (
        <div className="state admin-success">Order placed successfully</div>
      )}
      <section className="order-success-header">
        <div className={`order-success-icon${isPaid ? " is-paid" : ""}`} aria-hidden="true">
          {isPaid ? "OK" : "..."}
        </div>
        <div className="order-success-copy">
          <h2>{isPaid ? "Payment received" : getOrderStatusLabel(order.status)}</h2>
          <p className="note">
            {isPaid
              ? "We emailed your receipt and are now preparing the next steps."
              : "This page auto-refreshes while your payment status updates."}
          </p>
        </div>
        <StatusBadge status={order.status} />
      </section>
      {shouldShowStatusMessage && <div className="state">{statusMessage}</div>}
      <div className="order-status-grid">
        <div className="order-status-main">
          <div className="order-meta-card">
            <div>
              <span className="label">Status</span>
              <StatusBadge status={order.status} />
            </div>
            <div>
              <span className="label">Order ID</span>
              <strong>{getDisplayOrderId(order.publicId)}</strong>
            </div>
            <div>
              <span className="label">Date</span>
              <strong>{orderDateText}</strong>
            </div>
          </div>
          <div className="order-items">
            <h2>Items in your receipt</h2>
            <ul>
              {order.items.length === 0 && (
                <li>
                  <span className="note">No orders yet.</span>
                </li>
              )}
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
        </div>
        <OrderSummaryCard
          title="Order summary"
          rows={[
            { label: "Subtotal", value: formatMoney(order.subtotalAmount, order.currency) },
            { label: "Discount", value: `-${formatMoney(order.discountAmount, order.currency)}` },
            { label: "Tax", value: formatMoney(order.taxAmount, order.currency) },
          ]}
          totalLabel="Total"
          totalValue={formatMoney(order.totalAmount, order.currency)}
          note={isPaid ? "Payment confirmed" : "Waiting for payment confirmation"}
          compactOnMobile={false}
        />
      </div>
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
      {showHostedCheckoutNotice && (
        <div className="checkout-actions">
          <div className="state">
            This order uses hosted checkout. Continue payment from the Checkout Session.
          </div>
          <Link
            className="button ghost"
            to={`/checkout/success?order=${order.publicId}`}
          >
            Check payment status
          </Link>
        </div>
      )}
      <InfoBanner
        title="What happens next"
        tone={isPaid ? "success" : "neutral"}
        items={
          isPaid
            ? [
                "We validate payment and reserve your items.",
                "You receive shipment tracking once your order is packed.",
                "Need help? Reply to your confirmation email anytime.",
              ]
            : [
                "Payment status syncs automatically from Stripe webhook updates.",
                "If pending persists, refresh status and verify the Checkout Session.",
              ]
        }
      />
      <div className="checkout-actions">
        <Link className="button ghost" to="/cart">
          Return to cart
        </Link>
        <Link className="button primary" to="/">
          Continue shopping
        </Link>
      </div>
    </section>
  );
}
