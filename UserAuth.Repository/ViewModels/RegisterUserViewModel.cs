using System.ComponentModel.DataAnnotations;
using UserAuth.Repository.CustomValidation;

namespace UserAuth.Repository.ViewModels;

public class RegisterUserViewModel
{

    [Required(ErrorMessage = "User name is required")]
    [MaxLength(50, ErrorMessage = "User name cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "User name can only contain letters and numbers")]
    public string UserName { get; set; } = null!;


    [Required(ErrorMessage = "Email is required")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;


    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
    public string PhoneNumber { get; set; } = null!;


    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters")]
    public string FirstName { get; set; } = null!;


    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name can only contain letters")]
    public string LastName { get; set; } = null!;


    [Required(ErrorMessage = "Password is required")]
    [MaxLength(40, ErrorMessage = "Password cannot exceed 40 characters")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 special character, 1 number, and be at least 8 characters long")]
    public string Password { get; set; } = null!;
    

    [Required(ErrorMessage = "Confirm password is required")]
    [MaxLength(40, ErrorMessage = "Confirm password cannot exceed 40 characters")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;


    [Required(ErrorMessage = "Role is required")]
    public int Role { get; set; } 


    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [MinimumAge(18, ErrorMessage = "You must be at least 18 years old to register.")]
    public DateTime DateOfBirth { get; set; } 

    public string? Address {get;set;}

    [Required(ErrorMessage = "zipcode is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid Zipcode, should be 6 numbers")]
    public int Pincode {get;set;}
}
