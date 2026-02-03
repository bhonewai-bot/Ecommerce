import { useEffect, useState } from "react";
import type { CartItem } from "./store";
import { getCart } from "./store";

export function useCart() {
  const [items, setItems] = useState<CartItem[]>(() => getCart().items);

  useEffect(() => {
    const handleUpdate = () => setItems(getCart().items);
    window.addEventListener("storage", handleUpdate);
    window.addEventListener("cart:updated", handleUpdate);
    return () => {
      window.removeEventListener("storage", handleUpdate);
      window.removeEventListener("cart:updated", handleUpdate);
    };
  }, []);

  const total = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const count = items.reduce((sum, item) => sum + item.quantity, 0);

  return { items, total, count };
}
