using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class RegisterUserRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}