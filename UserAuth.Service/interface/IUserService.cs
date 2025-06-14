using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;

namespace UserAuth.Service.interfaces;

public interface IUserService 
{
    Task<ResponsesViewModel?> UserLogin(LoginViewModel model);

    ResponseTokenViewModel Validate2faToken(User2FAViewModel model);

    Task<List<User>?> GetAllUserDetails();
    Task<ResponsesViewModel> ForgetPassword(EmailViewModel email);
    ResponsesViewModel ValidateResetPasswordToken(string token);
    
    // Method to reset the password
    ResponsesViewModel ResetPassword(ForgetPasswordViewModel model);
    
    // Method for registering a new user
    Task<ResponsesViewModel> RegisterUser(RegisterUserViewModel model);

    // method for getting user by id
    User GetUserById(int userId);
    
    // soft delete user
    ResponsesViewModel DeleteUser(int userId, string email);

    // method for updating user details
    ResponsesViewModel UpdateUser(User user, string email);

    // users log history
    List<UsersHistory> LogUserHistory();

    // method for deleting all auth token which are generated for 2 factor authentication
    bool Delete2FaAuth(string Email);
    

}
