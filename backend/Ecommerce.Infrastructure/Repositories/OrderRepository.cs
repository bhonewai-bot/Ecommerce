using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Checkout.Models;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly EcommerceDbContext _db;

    public OrderRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task<Result> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var entity = new order
        {
            public_id = command.PublicId,
            status = (short)command.Status,
            subtotal_amount = command.SubtotalAmount,
            discount_amount = command.DiscountAmount,
            tax_amount = command.TaxAmount,
            total_amount = command.TotalAmount,
            currency = command.Currency,
            customer_email = command.CustomerEmail,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        foreach (var item in command.Items)
        {
            entity.order_items.Add(new order_item
            {
                product_id = item.ProductId,
                product_name = item.ProductName,
                unit_price = item.UnitPrice,
                quantity = item.Quantity,
                line_total = item.LineTotal
            });
        }

        _db.orders.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<GetOrderResponse>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var item = await _db.orders
            .AsNoTracking()
            .Where(o => o.public_id == publicId)
            .Select(o => new GetOrderResponse
            {
                PublicId = o.public_id,
                Status = (OrderStatus)o.status,
                Currency = o.currency,
                SubtotalAmount = o.subtotal_amount,
                DiscountAmount = o.discount_amount,
                TaxAmount = o.tax_amount,
                TotalAmount = o.total_amount,
                Items = o.order_items
                    .OrderBy(oi => oi.id)
                    .Select(oi => new OrderItemDto
                    {
                        ProductId = oi.product_id ?? 0,
                        ProductName = oi.product_name,
                        UnitPrice = oi.unit_price,
                        Quantity = oi.quantity,
                        LineTotal = oi.line_total
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? Result<GetOrderResponse>.NotFound() : Result<GetOrderResponse>.Ok(item);
    }
}
