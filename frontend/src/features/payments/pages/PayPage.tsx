import { useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { CardElement, Elements, useElements, useStripe } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { useNavigate, useParams } from "react-router-dom";
import { useCreatePaymentIntent } from "../queries";
import { getApiErrorMessage } from "../../../shared/utils/errorMessages";
import { useOrder } from "../../orders/queries";
import OrderSummaryCard from "../../../shared/components/checkout/OrderSummaryCard";
import InfoBanner from "../../../shared/components/checkout/InfoBanner";
import { getDisplayOrderId } from "../../../shared/utils/orderId";

const stripePromise = loadStripe(
  import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || ""
);

function PaymentForm({
  publicId,
  clientSecret,
  submitLabel,
}: {
  publicId: string;
  clientSecret: string;
  submitLabel: string;
}) {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!stripe || !elements) return;

    setError("");
    setIsSubmitting(true);

    const cardElement = elements.getElement(CardElement);
    if (!cardElement) {
      setError("Payment form is not ready.");
      setIsSubmitting(false);
      return;
    }

    const result = await stripe.confirmCardPayment(clientSecret, {
      payment_method: { card: cardElement },
    });

    if (result.error) {
      setError(result.error.message ?? "Payment failed.");
      setIsSubmitting(false);
      return;
    }

    if (result.paymentIntent?.status === "succeeded") {
      navigate(`/orders/${publicId}`, { state: { paymentSuccess: true } });
      return;
    }

    setError("Payment is still processing.");
    setIsSubmitting(false);
  };

  return (
    <form className="checkout-card payment-card" onSubmit={handleSubmit}>
      <h2>Complete payment</h2>
      <p className="note">Use a test card like 4242 4242 4242 4242.</p>
      <div className="card-input">
        <CardElement options={{ hidePostalCode: true }} />
      </div>
      {error && <div className="state error">{error}</div>}
      <button className="button primary summary-cta" type="submit" disabled={!stripe || isSubmitting}>
        {isSubmitting ? "Processing..." : submitLabel}
      </button>
    </form>
  );
}

export default function PayPage() {
  const { publicId } = useParams();
  const orderQuery = useOrder(publicId);
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [error, setError] = useState("");
  const intentMutation = useCreatePaymentIntent();
  const [requestedPublicId, setRequestedPublicId] = useState<string | null>(null);
  const [idempotencyKey, setIdempotencyKey] = useState<string | null>(null);
  const resolvedClientSecret =
    clientSecret ?? intentMutation.data?.clientSecret ?? null;

  const canLoadStripe = useMemo(
    () => Boolean(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY),
    []
  );

  useEffect(() => {
    if (!publicId) return;
    setClientSecret(null);
    setError("");
    setRequestedPublicId(null);
    setIdempotencyKey(crypto.randomUUID());
  }, [publicId]);

  useEffect(() => {
    if (!intentMutation.data?.clientSecret) return;
    if (clientSecret === intentMutation.data.clientSecret) return;
    setClientSecret(intentMutation.data.clientSecret);
  }, [intentMutation.data?.clientSecret, clientSecret]);

  useEffect(() => {
    if (!publicId) return;
    if (orderQuery.isLoading) return;
    if (orderQuery.data?.hasCheckoutSession) return;
    if (requestedPublicId === publicId) return;
    if (intentMutation.isPending) return;
    if (!idempotencyKey) return;

    setRequestedPublicId(publicId);
    intentMutation.mutate({ publicId, idempotencyKey }, {
      onSuccess: (data) => {
        setClientSecret(data.clientSecret);
      },
      onError: (err) => {
        setError(getApiErrorMessage(err, "Failed to start payment. Please try again."));
      },
    });
  }, [
    publicId,
    requestedPublicId,
    intentMutation,
    idempotencyKey,
    orderQuery.isLoading,
    orderQuery.data?.hasCheckoutSession,
  ]);

  if (!publicId) {
    return <div className="state error">Missing order ID.</div>;
  }

  if (!canLoadStripe) {
    return (
      <div className="state error">
        Missing Stripe publishable key. Set VITE_STRIPE_PUBLISHABLE_KEY.
      </div>
    );
  }

  const order = orderQuery.data;
  if (order?.hasCheckoutSession) {
    return (
      <section className="pay-page">
        <div className="page-heading">
          <h1>Hosted checkout required</h1>
          <p className="subtitle">
            This order was created with Stripe Checkout and cannot be paid on
            this page.
          </p>
        </div>
        <div className="state">
          Continue payment from the hosted checkout flow.
        </div>
        <div className="checkout-actions">
          <button
            className="button ghost"
            type="button"
            onClick={() =>
              navigate(`/checkout/success?order=${order.publicId}`)
            }
          >
            Check payment status
          </button>
          <button className="button primary" type="button" onClick={() => navigate("/cart")}>
            Return to cart
          </button>
        </div>
      </section>
    );
  }

  const paymentLabel = order
    ? `Pay ${order.currency} ${order.totalAmount.toFixed(2)}`
    : "Pay";

  return (
    <section className="pay-page">
      <div className="page-heading">
        <h1>Secure checkout</h1>
        <p className="subtitle">Complete your payment to finalize the order.</p>
      </div>
      {order && (
        <div className="payment-grid">
          <div className="payment-main">
            <section className="payment-context">
              <h2>Payment details</h2>
              <div className="payment-context-grid">
                <div>
                  <span className="label">Order ID</span>
                  <strong>{getDisplayOrderId(order.publicId)}</strong>
                </div>
                <div>
                  <span className="label">Currency</span>
                  <strong>{order.currency}</strong>
                </div>
                <div>
                  <span className="label">Amount due</span>
                  <strong>{order.currency} {order.totalAmount.toFixed(2)}</strong>
                </div>
              </div>
            </section>
            {error && <div className="state error">{error}</div>}
            {intentMutation.isPending && !error && (
              <div className="state">Creating payment...</div>
            )}
            {!intentMutation.isPending && !resolvedClientSecret && !error && (
              <div className="state">Preparing payment details...</div>
            )}
            {resolvedClientSecret && (
              <Elements stripe={stripePromise}>
                <PaymentForm publicId={publicId} clientSecret={resolvedClientSecret} submitLabel={paymentLabel} />
              </Elements>
            )}
          </div>
          <OrderSummaryCard
            rows={[
              { label: "Order", value: getDisplayOrderId(order.publicId) },
              { label: "Currency", value: order.currency },
            ]}
            totalLabel="Amount due"
            totalValue={`${order.currency} ${order.totalAmount.toFixed(2)}`}
            note="Includes taxes & fees"
            cta={
              <button className="button primary summary-cta" type="button" disabled>
                {paymentLabel}
              </button>
            }
            footer={
              <InfoBanner
                title="Secure payment"
                items={["Encrypted card details", "Powered by Stripe", "Instant order confirmation"]}
              />
            }
          />
        </div>
      )}
      {!order && (
        <>
          {error && <div className="state error">{error}</div>}
          {orderQuery.isLoading && <div className="state">Loading order details...</div>}
          {!orderQuery.isLoading && intentMutation.isPending && !error && (
            <div className="state">Creating payment...</div>
          )}
          {!intentMutation.isPending && !resolvedClientSecret && !error && (
            <div className="state">Preparing payment details...</div>
          )}
          {resolvedClientSecret && (
            <Elements stripe={stripePromise}>
              <PaymentForm publicId={publicId} clientSecret={resolvedClientSecret} submitLabel={paymentLabel} />
            </Elements>
          )}
        </>
      )}
    </section>
  );
}
