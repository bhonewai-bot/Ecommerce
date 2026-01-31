import { useEffect, useMemo, useRef, useState } from "react";
import { CardElement, Elements, useElements, useStripe } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { useNavigate, useParams } from "react-router-dom";
import { useCreatePaymentIntent } from "../queries";

const stripePromise = loadStripe(
  import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || ""
);

function PaymentForm({ publicId, clientSecret }: { publicId: string; clientSecret: string }) {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
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
      navigate(`/orders/${publicId}`);
      return;
    }

    setError("Payment is still processing.");
    setIsSubmitting(false);
  };

  return (
    <form className="checkout-card" onSubmit={handleSubmit}>
      <h2>Complete payment</h2>
      <p className="note">Use a test card like 4242 4242 4242 4242.</p>
      <div className="card-input">
        <CardElement options={{ hidePostalCode: true }} />
      </div>
      {error && <div className="state error">{error}</div>}
      <button className="button primary" type="submit" disabled={!stripe || isSubmitting}>
        {isSubmitting ? "Processing..." : "Pay"}
      </button>
    </form>
  );
}

export default function PayPage() {
  const { publicId } = useParams();
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [error, setError] = useState("");
  const intentMutation = useCreatePaymentIntent();
  const requestedRef = useRef(false);

  const canLoadStripe = useMemo(
    () => Boolean(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY),
    []
  );

  useEffect(() => {
    if (!publicId || requestedRef.current) return;
    requestedRef.current = true;
    intentMutation.mutate(publicId, {
      onSuccess: (data) => setClientSecret(data.clientSecret),
      onError: (err) => {
        setError(err instanceof Error ? err.message : "Failed to start payment.");
      },
    });
  }, [publicId, intentMutation]);

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

  return (
    <section className="pay-page">
      <div className="page-heading">
        <h1>Secure checkout</h1>
        <p className="subtitle">Complete your payment to finalize the order.</p>
      </div>
      {error && <div className="state error">{error}</div>}
      {!clientSecret && !error && (
        <div className="state">Preparing payment intent...</div>
      )}
      {clientSecret && (
        <Elements stripe={stripePromise}>
          <PaymentForm publicId={publicId} clientSecret={clientSecret} />
        </Elements>
      )}
    </section>
  );
}
