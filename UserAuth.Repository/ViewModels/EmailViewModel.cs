using System.ComponentModel.DataAnnotations;

namespace UserAuth.Repository.ViewModels;

public class EmailViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email is not valid")]
    public string? ToEmail { get; set; }
}

public class User2FAViewModel
{
    public string? ToEmail { get; set; }

    [Required(ErrorMessage = "Code is required")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Code is not valid")]
    public string? Token { get; set; }
}
