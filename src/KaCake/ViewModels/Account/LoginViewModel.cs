using System.ComponentModel.DataAnnotations;

namespace KaCake.ViewModels.Account
{
    public class LoginViewModel
    {
        public string ReturnURL { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
