import { useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, useSearchParams } from "react-router-dom";
import type { Order } from "../../../shared/types/api";
import { cancelOrder, getOrderByCheckoutSession, getOrderByPublicId } from "../../orders/api";
import { updatePendingOrderStatus } from "../../cart/store";

export default function CheckoutCancelPage() {
  const [searchParams] = useSearchParams();
  const orderParam = searchParams.get("order");
  const sessionId = searchParams.get("session_id");
  const [cancelError, setCancelError] = useState("");
  const hasCancelledRef = useRef(false);

  const orderQuery = useQuery<Order>({
    queryKey: ["orders", "checkout-cancel", orderParam, sessionId],
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
  });

  useEffect(() => {
    if (!orderQuery.data || hasCancelledRef.current) return;

    if (orderQuery.data.status !== "PendingPayment") {
      updatePendingOrderStatus(
        orderQuery.data.status === "Paid" ? "paid" : "canceled"
      );
      hasCancelledRef.current = true;
      return;
    }

    cancelOrder(orderQuery.data.publicId)
      .then(() => {
        updatePendingOrderStatus("canceled");
      })
      .catch((err) => {
        setCancelError(
          err instanceof Error ? err.message : "Unable to cancel order."
        );
      })
      .finally(() => {
        hasCancelledRef.current = true;
      });
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
    return <div className="state">Loading checkout details...</div>;
  }

  if (orderQuery.isError) {
    return (
      <div className="state error">
        We could not load your order.{" "}
        <Link className="button ghost" to="/cart">
          Return to cart
        </Link>
      </div>
    );
  }

  return (
    <section className="order-page">
      <div className="page-heading">
        <h1>Checkout canceled</h1>
        <p className="subtitle">
          Your cart is still here. You can try again whenever you’re ready.
        </p>
      </div>
      {cancelError && <div className="state error">{cancelError}</div>}
      <div className="checkout-actions">
        <Link className="button primary" to="/cart">
          Return to cart
        </Link>
        <Link className="button ghost" to="/">
          Continue shopping
        </Link>
      </div>
    </section>
  );
}
