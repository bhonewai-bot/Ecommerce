namespace Ecommerce.Application.Orders;

public enum OrderStatus : short
{
    PendingPayment = 0,
    Paid = 1,
    Cancelled = 2,
    Fulfilled = 3
}
