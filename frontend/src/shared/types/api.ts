export interface Category {
  id: number;
  name: string;
  description?: string | null;
}

export interface Product {
  id: number;
  categoryId: number;
  name: string;
  description?: string | null;
  price: number;
  imageUrl?: string | null;
}

export type OrderStatus = "PendingPayment" | "Paid" | "Cancelled" | "Fulfilled";

export interface OrderItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  publicId: string;
  status: OrderStatus;
  currency: string;
  subtotalAmount: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  hasCheckoutSession: boolean;
  items: OrderItem[];
}

export interface AdminOrderListItem {
  publicId: string;
  status: OrderStatus;
  currency: string;
  totalAmount: number;
  customerEmail?: string | null;
  createdAt: string;
}

export interface PaymentIntentResponse {
  clientSecret: string;
}

export interface CheckoutItemInput {
  productId: number;
  quantity: number;
}

export interface CheckoutRequest {
  customerEmail?: string | null;
  items: CheckoutItemInput[];
}

export interface CheckoutResponse {
  orderPublicId: string;
  totalAmount: number;
  currency: string;
  status: OrderStatus;
  items: OrderItem[];
  stripeCheckoutUrl?: string | null;
  checkoutSessionId?: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export type CreateProductInput = Omit<Product, "id">;
export type UpdateProductInput = Omit<Product, "id">;
export type CreateCategoryInput = Omit<Category, "id">;
export type UpdateCategoryInput = Omit<Category, "id">;
