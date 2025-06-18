using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Ім'я' є обов'язковим.")]
        [StringLength(100, ErrorMessage = "Ім'я не може перевищувати 100 символів.")]
        [Display(Name = "Ваше ім'я")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Поле 'Email' є обов'язковим.")]
        [EmailAddress(ErrorMessage = "Введіть коректний Email.")]
        [StringLength(100, ErrorMessage = "Email не може перевищувати 100 символів.")]
        [Display(Name = "Ваш Email")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Поле 'Телефон' є обов'язковим.")]
        
        [RegularExpression(@"^\+?3?8?0?\s*\(?\d{2,3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$",
            ErrorMessage = "Введіть коректний номер телефону в українському форматі (наприклад, +380501234567, 050 123 45 67, 0(44)123-45-67).")]
        [StringLength(20, ErrorMessage = "Номер телефону не може перевищувати 20 символів.")] 
        [Display(Name = "Ваш телефон")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Поле 'Адреса доставки' є обов'язковим.")]
        [StringLength(200, ErrorMessage = "Адреса доставки не може перевищувати 200 символів.")]
        [Display(Name = "Адреса доставки")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Дата замовлення")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Загальна сума")]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}