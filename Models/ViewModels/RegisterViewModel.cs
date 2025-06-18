
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ім'я користувача є обов'язковим")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно бути від 3 до 50 символів")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль повинен бути мінімум 6 символів")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; }
    }
}