using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Features.Categories.Models;

public sealed class CreateCategoryRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
