import { useMemo } from "react";
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

function formatMoney(value: number, currency: string) {
  return `${currency} ${value.toFixed(2)}`;
}

function OrderSummary({ order }: { order: Order }) {
  return (
    <div className="order-summary receipt-breakdown">
      <div className="order-summary-row">
        <span className="label">Subtotal</span>
        <strong>{formatMoney(order.subtotalAmount, order.currency)}</strong>
      </div>
      <div className="order-summary-row">
        <span className="label">Discount</span>
        <strong>-{formatMoney(order.discountAmount, order.currency)}</strong>
      </div>
      <div className="order-summary-row">
        <span className="label">Tax</span>
        <strong>{formatMoney(order.taxAmount, order.currency)}</strong>
      </div>
      <div className="order-summary-row order-total">
        <span className="label">Total</span>
        <strong>{formatMoney(order.totalAmount, order.currency)}</strong>
      </div>
    </div>
  );
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
        <h1>Order {order.publicId}</h1>
        <p className="subtitle">Track payment and fulfillment status.</p>
      </div>
      {paymentSuccess && (
        <div className="state admin-success">Order placed successfully</div>
      )}
      <section className="order-success-header">
        <div className="order-success-icon" aria-hidden="true">
          OK
        </div>
        <div>
          <h2>{order.status === "Paid" ? "Payment received" : getOrderStatusLabel(order.status)}</h2>
          <p className="note">We emailed your receipt and are now preparing the next steps.</p>
        </div>
      </section>
      {shouldShowStatusMessage && <div className="state">{statusMessage}</div>}
      <div className="order-meta-card">
        <div>
          <span className="label">Status</span>
          <StatusBadge status={order.status} />
        </div>
        <div>
          <span className="label">Order ID</span>
          <strong>{order.publicId}</strong>
        </div>
        <div>
          <span className="label">Date</span>
          <strong>{orderDateText}</strong>
        </div>
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
        tone="success"
        items={[
          "We validate payment and reserve your items.",
          "You receive shipment tracking once your order is packed.",
          "Need help? Reply to your confirmation email anytime.",
        ]}
      />
      <OrderSummary order={order} />
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
    </section>
  );
}
