
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
        public string? Address {  get; set; }
    }
}
