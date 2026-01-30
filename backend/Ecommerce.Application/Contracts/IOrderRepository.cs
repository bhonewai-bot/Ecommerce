using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Application.Features.Payments.Models;

namespace Ecommerce.Application.Contracts;

public interface IOrderRepository
{
    Task<Result> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result<OrderPaymentInfoDto>> GetPaymentInfoByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result> MarkPaidByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
