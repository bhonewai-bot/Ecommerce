import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, useSearchParams } from "react-router-dom";
import type { Order } from "../../../shared/types/api";
import { getOrderByCheckoutSession, getOrderByPublicId } from "../../orders/api";
import {
  clearCart,
  clearPendingOrder,
  setLastOrderId,
  updatePendingOrderStatus,
} from "../../cart/store";
import StatusBadge from "../../../shared/components/checkout/StatusBadge";
import InfoBanner from "../../../shared/components/checkout/InfoBanner";
import OrderSummaryCard from "../../../shared/components/checkout/OrderSummaryCard";
import {
  getOrderStatusLabel,
  getOrderStatusMessage,
} from "../../../shared/utils/orderStatus";
import { getDisplayOrderId } from "../../../shared/utils/orderId";

function formatMoney(value: number, currency: string) {
  return `${currency} ${value.toFixed(2)}`;
}

export default function CheckoutSuccessPage() {
  const [searchParams] = useSearchParams();
  const orderParam = searchParams.get("order");
  const sessionId = searchParams.get("session_id");

  const orderQuery = useQuery<Order>({
    queryKey: ["orders", "checkout-success", orderParam, sessionId],
    queryFn: () => {
      if (orderParam) {
        return getOrderByPublicId(orderParam);
      }
      if (sessionId) {
        return getOrderByCheckoutSession(sessionId);
      }
      throw new Error("Missing order reference.");
    },
    enabled: Boolean(orderParam || sessionId),
    refetchInterval: (query) =>
      query.state.data?.status === "PendingPayment" ? 3000 : false,
  });

  useEffect(() => {
    if (!orderQuery.data) return;
    setLastOrderId(orderQuery.data.publicId);
    if (orderQuery.data.status === "Paid") {
      clearCart();
      clearPendingOrder();
      return;
    }
    if (orderQuery.data.status === "Cancelled") {
      updatePendingOrderStatus("canceled");
    }
  }, [orderQuery.data]);

  if (!orderParam && !sessionId) {
    return (
      <div className="state error">
        Missing order reference.{" "}
        <Link className="button ghost" to="/cart">
          Return to cart
        </Link>
      </div>
    );
  }

  if (orderQuery.isLoading) {
    return <div className="state">Confirming payment...</div>;
  }

  if (orderQuery.isError || !orderQuery.data) {
    return (
      <div className="state error">
        We could not load your order.{" "}
        <Link className="button ghost" to="/cart">
          Return to cart
        </Link>
      </div>
    );
  }

  const order = orderQuery.data;
  const isPaid = order.status === "Paid";
  const isPending = order.status === "PendingPayment";
  const isCancelled = order.status === "Cancelled";

  return (
    <section className="order-page">
      <div className="page-heading">
        <span className="order-kicker">Checkout status</span>
        <h1>{isPaid ? "Payment received" : "Checkout in progress"}</h1>
        <p className="subtitle">
          {isPaid
            ? "We have confirmed your payment and are preparing your order."
            : "We are still waiting for payment confirmation from Stripe."}
        </p>
      </div>

      <section className="order-success-header">
        <div className={`order-success-icon${isPaid ? " is-paid" : ""}`} aria-hidden="true">
          {isPaid ? "OK" : "..."}
        </div>
        <div className="order-success-copy">
          <h2>{isPaid ? "Payment confirmed" : getOrderStatusLabel(order.status)}</h2>
          <p className="note">
            {isPaid
              ? "Your receipt is ready. We will notify you when fulfillment starts."
              : getOrderStatusMessage(order.status)}
          </p>
        </div>
        <StatusBadge status={order.status} />
      </section>

      {isPaid && <div className="state admin-success">Payment confirmed.</div>}
      {isPending && (
        <div className="state">Awaiting payment confirmation. This page refreshes automatically.</div>
      )}
      {isCancelled && <div className="state error">This checkout was canceled.</div>}

      <div className="order-status-grid">
        <div className="order-status-main">
          <div className="order-meta-card">
            <div>
              <span className="label">Order ID</span>
              <strong>{getDisplayOrderId(order.publicId)}</strong>
            </div>
            <div>
              <span className="label">Payment status</span>
              <StatusBadge status={order.status} />
            </div>
            <div>
              <span className="label">Currency</span>
              <strong>{order.currency}</strong>
            </div>
          </div>
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
        </div>
        <OrderSummaryCard
          title="Receipt summary"
          rows={[
            { label: "Subtotal", value: formatMoney(order.subtotalAmount, order.currency) },
            { label: "Tax", value: formatMoney(order.taxAmount, order.currency) },
          ]}
          totalLabel="Total"
          totalValue={formatMoney(order.totalAmount, order.currency)}
          note={isPaid ? "Paid via Stripe Checkout" : "Pending Stripe confirmation"}
          compactOnMobile={false}
        />
      </div>

      <InfoBanner
        title="What happens next"
        tone={isPaid ? "success" : "neutral"}
        items={
          isPaid
            ? [
                "Payment is confirmed and order is queued for fulfillment.",
                "You can return to cart or review full order details anytime.",
              ]
            : [
                "Stripe may finalize status a few seconds after redirect.",
                "Use Check status if this page stays pending for too long.",
              ]
        }
      />

      <div className="checkout-actions">
        {isPending && (
          <button
            className="button primary"
            type="button"
            onClick={() => orderQuery.refetch()}
          >
            Check status
          </button>
        )}
        {!isPending && (
          <Link className="button primary" to={`/orders/${order.publicId}`}>
            View order
          </Link>
        )}
        <Link className="button ghost" to="/cart">
          Return to cart
        </Link>
      </div>
    </section>
  );
}
