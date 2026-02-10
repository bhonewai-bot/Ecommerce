import { useEffect, useState } from "react";
import type { PendingOrder } from "./store";
import { getPendingOrder, PENDING_ORDER_EVENT } from "./store";

export function usePendingOrder() {
  const [pendingOrder, setPendingOrder] = useState<PendingOrder | null>(() =>
    getPendingOrder()
  );

  useEffect(() => {
    const handleUpdate = () => setPendingOrder(getPendingOrder());
    window.addEventListener("storage", handleUpdate);
    window.addEventListener(PENDING_ORDER_EVENT, handleUpdate);
    return () => {
      window.removeEventListener("storage", handleUpdate);
      window.removeEventListener(PENDING_ORDER_EVENT, handleUpdate);
    };
  }, []);

  return pendingOrder;
}
