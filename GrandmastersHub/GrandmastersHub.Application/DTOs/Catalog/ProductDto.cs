namespace GrandmastersHub.Application.DTOs.Catalog
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public IReadOnlyList<string> ImageUrls { get; set; } = [];
        public IReadOnlyList<ProductVariantDto> Variants { get; set; } = [];
    }
    public sealed record ProductVariantDto(int ProductVariantId, string Name, decimal Price, int StockQuantity);
}
