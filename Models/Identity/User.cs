
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models.Identity
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ім'я користувача є обов'язковим")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно бути від 3 до 50 символів")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [StringLength(256)] 
        public string PasswordHash { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "User"; 
    }
}