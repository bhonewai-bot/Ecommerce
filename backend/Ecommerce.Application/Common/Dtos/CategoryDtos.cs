using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Common.Dtos;

public sealed record CategoryDto(int Id, string Name, string? Description);

public sealed class CategoryCreateDto
{
    [Required, StringLength(150)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}

public sealed class CategoryUpdateDto
{
    [Required, StringLength(150)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
