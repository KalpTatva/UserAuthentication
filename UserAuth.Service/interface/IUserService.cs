using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;

namespace UserAuth.Service.interfaces;

public interface IUserService 
{
    Task<ResponseTokenViewModel> UserLogin(LoginViewModel model);
    Task<List<User>?> GetAllUserDetails();
    Task<ResponsesViewModel> ForgetPassword(EmailViewModel email);
    ResponsesViewModel ValidateResetPasswordToken(string token);
    
    // Method to reset the password
    ResponsesViewModel ResetPassword(ForgetPasswordViewModel model);

}
