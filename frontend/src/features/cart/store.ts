export type CartItem = {
  productId: number;
  name: string;
  price: number;
  quantity: number;
};

export type PendingOrderStatus =
  | "pending"
  | "paid"
  | "failed"
  | "expired"
  | "canceled";

export type PendingOrder = {
  pendingOrderId: string;
  checkoutSessionId?: string | null;
  checkoutUrl?: string | null;
  createdAt: string;
  cartSnapshot: CartItem[];
  status: PendingOrderStatus;
};

type CartState = {
  items: CartItem[];
};

const STORAGE_KEY = "evercart:cart";
const PENDING_ORDER_KEY = "evercart:pending-order";
const LAST_ORDER_KEY = "evercart:last-order-id";
export const PENDING_ORDER_EVENT = "pending-order:updated";
export const LAST_ORDER_EVENT = "last-order:updated";

function readCart(): CartState {
  if (typeof window === "undefined") {
    return { items: [] };
  }
  const raw = window.localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return { items: [] };
  }
  try {
    const parsed = JSON.parse(raw) as CartState;
    if (!parsed.items) {
      return { items: [] };
    }
    return parsed;
  } catch {
    return { items: [] };
  }
}

function writeCart(state: CartState) {
  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  window.dispatchEvent(new Event("cart:updated"));
}

function readPendingOrder(): PendingOrder | null {
  if (typeof window === "undefined") {
    return null;
  }
  const raw = window.localStorage.getItem(PENDING_ORDER_KEY);
  if (!raw) {
    return null;
  }
  try {
    const parsed = JSON.parse(raw) as PendingOrder;
    if (!parsed || !parsed.pendingOrderId || !parsed.cartSnapshot) {
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

function writePendingOrder(pendingOrder: PendingOrder | null) {
  if (typeof window === "undefined") {
    return;
  }
  if (!pendingOrder) {
    window.localStorage.removeItem(PENDING_ORDER_KEY);
  } else {
    window.localStorage.setItem(PENDING_ORDER_KEY, JSON.stringify(pendingOrder));
  }
  window.dispatchEvent(new Event(PENDING_ORDER_EVENT));
}

function writeLastOrderId(publicId: string | null) {
  if (typeof window === "undefined") {
    return;
  }
  if (!publicId) {
    window.localStorage.removeItem(LAST_ORDER_KEY);
  } else {
    window.localStorage.setItem(LAST_ORDER_KEY, publicId);
  }
  window.dispatchEvent(new Event(LAST_ORDER_EVENT));
}

export function getCart(): CartState {
  return readCart();
}

export function getPendingOrder(): PendingOrder | null {
  return readPendingOrder();
}

export function setPendingOrder(pendingOrder: PendingOrder) {
  writePendingOrder(pendingOrder);
}

export function getLastOrderId(): string | null {
  if (typeof window === "undefined") {
    return null;
  }
  return window.localStorage.getItem(LAST_ORDER_KEY);
}

export function setLastOrderId(publicId: string) {
  writeLastOrderId(publicId);
}

export function clearLastOrderId() {
  writeLastOrderId(null);
}

export function updatePendingOrderStatus(status: PendingOrderStatus) {
  const current = readPendingOrder();
  if (!current) return;
  writePendingOrder({ ...current, status });
}

export function clearPendingOrder() {
  writePendingOrder(null);
}

export function addToCart(item: Omit<CartItem, "quantity">, quantity = 1) {
  const current = readCart();
  const existing = current.items.find((entry) => entry.productId === item.productId);
  if (existing) {
    existing.quantity += quantity;
  } else {
    current.items.push({ ...item, quantity });
  }
  writeCart(current);
}

export function updateQuantity(productId: number, quantity: number) {
  const current = readCart();
  if (quantity <= 0) {
    current.items = current.items.filter((entry) => entry.productId !== productId);
  } else {
    current.items = current.items.map((entry) =>
      entry.productId === productId ? { ...entry, quantity } : entry
    );
  }
  writeCart(current);
}

export function removeItem(productId: number) {
  const current = readCart();
  current.items = current.items.filter((entry) => entry.productId !== productId);
  writeCart(current);
}

export function clearCart() {
  writeCart({ items: [] });
}
