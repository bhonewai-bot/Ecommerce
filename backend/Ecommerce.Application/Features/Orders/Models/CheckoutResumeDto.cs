namespace Ecommerce.Application.Features.Orders.Models;

public sealed record CheckoutResumeDto(
    Guid OrderPublicId,
    string CheckoutUrl);
