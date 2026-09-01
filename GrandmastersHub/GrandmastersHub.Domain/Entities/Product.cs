using System;
using System.Collections.Generic;
using System.Text;

namespace GrandmastersHub.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
