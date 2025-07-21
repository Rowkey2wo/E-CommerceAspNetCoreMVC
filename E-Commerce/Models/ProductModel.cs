using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class ProductModel
    {
        
        [Key] public int ProductId { get; set; }

        [Required] public string FoodName { get; set; } = string.Empty;

        [Required] public string Category { get; set; } = string.Empty;

        [Required] public string Description { get; set; } = string.Empty;

        [Required] public decimal Price { get; set; }

        public string? Image { get; set; }

        [Required] public int Stock { get; set; }
    }
}
