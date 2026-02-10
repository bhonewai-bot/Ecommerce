import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import type { Order } from "../../../shared/types/api";
import {
  cancelOrder,
  getOrderByCheckoutSession,
  getOrderByPublicId,
  getResumableCheckoutBySession,
} from "../../orders/api";
import { updatePendingOrderStatus } from "../../cart/store";
import { usePendingOrder } from "../../cart/usePendingOrder";

export default function CheckoutCancelPage() {
  const navigate = useNavigate();
  const pendingOrder = usePendingOrder();
  const [searchParams] = useSearchParams();
  const orderParam = searchParams.get("order");
  const sessionId = searchParams.get("session_id");
  const [cancelError, setCancelError] = useState("");
  const [resumeError, setResumeError] = useState("");
  const [isResuming, setIsResuming] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);

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

  const canContinuePayment = useMemo(
    () => orderQuery.data?.status === "PendingPayment",
    [orderQuery.data?.status]
  );
  const resumeCheckoutUrl = useMemo(() => {
    if (!orderQuery.data || !pendingOrder?.checkoutUrl) {
      return null;
    }
    return pendingOrder.pendingOrderId === orderQuery.data.publicId
      ? pendingOrder.checkoutUrl
      : null;
  }, [orderQuery.data, pendingOrder]);

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

  const order = orderQuery.data;
  const handleCancelOrder = async () => {
    if (!order || order.status !== "PendingPayment") return;
    setCancelError("");
    setIsCancelling(true);
    try {
      await cancelOrder(order.publicId);
      updatePendingOrderStatus("canceled");
      await orderQuery.refetch();
    } catch (err) {
      setCancelError(
        err instanceof Error ? err.message : "Unable to cancel order."
      );
    } finally {
      setIsCancelling(false);
    }
  };

  const handleContinuePayment = async () => {
    if (!order || order.status !== "PendingPayment") return;

    setResumeError("");
    if (sessionId) {
      setIsResuming(true);
      try {
        const resume = await getResumableCheckoutBySession(sessionId);
        window.location.href = resume.checkoutUrl;
        return;
      } catch (err) {
        setResumeError(
          err instanceof Error
            ? err.message
            : "Unable to resume checkout session."
        );
      } finally {
        setIsResuming(false);
      }
    }

    if (resumeCheckoutUrl) {
      window.location.href = resumeCheckoutUrl;
      return;
    }

    navigate("/cart");
  };

  return (
    <section className="order-page">
      <div className="page-heading">
        <h1>{canContinuePayment ? "Checkout not completed" : "Checkout canceled"}</h1>
        <p className="subtitle">
          {canContinuePayment
            ? "No payment was submitted. You can continue from hosted checkout or return to cart."
            : "Your cart is still here. You can try again whenever you’re ready."}
        </p>
      </div>
      {canContinuePayment && (
        <div className="state">
          Your order is still pending payment.
        </div>
      )}
      {cancelError && <div className="state error">{cancelError}</div>}
      {resumeError && <div className="state error">{resumeError}</div>}
      <div className="checkout-actions">
        {canContinuePayment && (
          <button
            className="button primary"
            type="button"
            onClick={handleContinuePayment}
            disabled={isResuming}
          >
            {isResuming ? "Resuming..." : "Continue payment"}
          </button>
        )}
        <Link className="button primary" to="/cart">
          Return to cart
        </Link>
        {canContinuePayment && (
          <button
            className="button danger"
            type="button"
            onClick={handleCancelOrder}
            disabled={isCancelling}
          >
            {isCancelling ? "Cancelling..." : "Cancel this order"}
          </button>
        )}
        <Link className="button ghost" to="/">
          Continue shopping
        </Link>
      </div>
    </section>
  );
}
