
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; 

namespace OnlineShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва продукту обов'язкова")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Ціна продукту обов'язкова")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна повинна бути більшою за 0")]
        public decimal Price { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; } 

        [NotMapped]
        public IFormFile? ImageFile { get; set; } 
    }
}