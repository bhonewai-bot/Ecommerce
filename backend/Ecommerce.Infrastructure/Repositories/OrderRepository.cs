using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Checkout.Models;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Application.Features.Payments.Models;
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

    public async Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var item = await _db.orders
            .AsNoTracking()
            .Where(o => o.public_id == publicId)
            .Select(o => new OrderDto(
                o.public_id,
                (OrderStatus)o.status,
                o.currency,
                o.subtotal_amount,
                o.discount_amount,
                o.tax_amount,
                o.total_amount,
                o.order_items
                    .OrderBy(oi => oi.id)
                    .Select(oi => new OrderItemDto(
                        oi.product_id ?? 0,
                        oi.product_name,
                        oi.unit_price,
                        oi.quantity,
                        oi.line_total))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? Result<OrderDto>.NotFound() : Result<OrderDto>.Ok(item);
    }

    public async Task<Result<OrderPaymentInfoDto>> GetPaymentInfoByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var item = await _db.orders
            .AsNoTracking()
            .Where(o => o.public_id == publicId)
            .Select(o => new OrderPaymentInfoDto(
                o.public_id,
                (OrderStatus)o.status,
                o.currency,
                o.total_amount))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? Result<OrderPaymentInfoDto>.NotFound() : Result<OrderPaymentInfoDto>.Ok(item);
    }

    public async Task<Result> MarkPaidByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var entity = await _db.orders
            .FirstOrDefaultAsync(o => o.public_id == publicId, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound();
        }

        if (entity.status == (short)OrderStatus.Paid)
        {
            return Result.Ok();
        }

        if (entity.status != (short)OrderStatus.PendingPayment)
        {
            return Result.Conflict("Order status is not payable.");
        }

        entity.status = (short)OrderStatus.Paid;
        entity.updated_at = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
