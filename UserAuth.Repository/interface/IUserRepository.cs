using UserAuth.Repository.Models;

namespace UserAuth.Repository.interfaces;

public interface IUserRepository
{
    User? GetUserByEmail(string email);
    User? GetUserById(int userId);
    Task<List<User>?> GetAllUserDetails();
    Task AddPasswordResetRequest(PasswordResetRequest passwordResetRequest);
    PasswordResetRequest? GetPasswordResetRequestByToken(string token);
    void UpdateUser(User user);
    void UpdatePasswordResetRequest(PasswordResetRequest passwordResetRequest);

}
