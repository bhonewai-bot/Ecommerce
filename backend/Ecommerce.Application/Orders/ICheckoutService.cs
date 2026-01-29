using Ecommerce.Application.Common;

namespace Ecommerce.Application.Orders;

public interface ICheckoutService
{
    Task<Result<CheckoutResponseDto>> CreateOrderAsync(
        CheckoutRequestDto request,
        CancellationToken cancellationToken);
}
