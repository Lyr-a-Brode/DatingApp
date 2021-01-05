using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class LoginUserRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}