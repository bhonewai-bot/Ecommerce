namespace Ecommerce.Application.Features.Checkout.Models;

public sealed class CheckoutRequest
{
    public string? CustomerEmail { get; set; }
    public List<CheckoutItemDto> Items { get; set; } = new();
}
