import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, useSearchParams } from "react-router-dom";
import type { Order } from "../../../shared/types/api";
import { getOrderByCheckoutSession, getOrderByPublicId } from "../../orders/api";
import { clearCart, clearPendingOrder, updatePendingOrderStatus } from "../../cart/store";

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

  return (
    <section className="order-page">
      <div className="page-heading">
        <h1>{isPaid ? "Payment received" : "Checkout in progress"}</h1>
        <p className="subtitle">
          {isPaid
            ? "We have confirmed your payment and are preparing your order."
            : "We are still waiting for payment confirmation."}
        </p>
      </div>
      {isPaid ? (
        <div className="state admin-success">Payment confirmed.</div>
      ) : (
        <div className="state">Awaiting payment confirmation...</div>
      )}
      <div className="order-summary">
        <div>
          <span className="label">Order ID</span>
          <strong>{order.publicId}</strong>
        </div>
        <div>
          <span className="label">Subtotal</span>
          <strong>{formatMoney(order.subtotalAmount, order.currency)}</strong>
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
      <div className="checkout-actions">
        <Link className="button ghost" to="/cart">
          Return to cart
        </Link>
        <Link className="button primary" to={`/orders/${order.publicId}`}>
          View order
        </Link>
      </div>
    </section>
  );
}
