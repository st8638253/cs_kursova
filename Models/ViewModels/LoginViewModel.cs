
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ім'я користувача є обов'язковим")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}