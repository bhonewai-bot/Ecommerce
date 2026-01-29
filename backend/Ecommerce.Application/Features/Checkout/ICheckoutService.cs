using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout.Models;

namespace Ecommerce.Application.Features.Checkout;

public interface ICheckoutService
{
    Task<Result<CheckoutResponse>> CreateOrderAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken);
}
