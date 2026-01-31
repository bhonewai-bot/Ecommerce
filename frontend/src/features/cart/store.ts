export type CartItem = {
  productId: number;
  name: string;
  price: number;
  quantity: number;
};

type CartState = {
  items: CartItem[];
};

const STORAGE_KEY = "evercart:cart";

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

export function getCart(): CartState {
  return readCart();
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
