using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;

namespace UserAuth.Repository.interfaces;

public interface IUserRepository
{
    User? GetUserByEmail(string email);
    User? GetUserById(int userId);
    Task<List<User>?> GetAllUserDetails();
    Task AddPasswordResetRequest(PasswordResetRequest passwordResetRequest);
    PasswordResetRequest? GetPasswordResetRequestByToken(string token);
    void UpdateUser(User user);
    public void AddUser2faAuth(User2faAuth user2FaAuth);
    public void UpdateUser2faAuth(User2faAuth user2FaAuth);
    void UpdatePasswordResetRequest(PasswordResetRequest passwordResetRequest);
    Task<bool> RegisterUser(RegisterUserViewModel model);
    bool DeleteUser(int userId,int DeleteUser);
    bool UpdateUserSP(User user,int userid);
    List<UsersHistory> GetUserHistroryLog();
    User2faAuth GetUser2faAuth(string email);
    void DeleteALlAuthTokenByEmail(string email);
    void SaveMessage(Message message);
}
