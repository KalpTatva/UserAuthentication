using Microsoft.EntityFrameworkCore;
using UserAuth.Repository.interfaces;
using UserAuth.Repository.Models;

namespace UserAuth.Repository.implementation;

public class UserRepository: IUserRepository
{

    private readonly UserAuthContext _context;
    public UserRepository(UserAuthContext context)
    {
        _context = context;
    }

    // DB call to get user by email
    public async Task<User?> GetUserByEmail(string email)
    {
        try
        {
            return await _context.Users.Where(x => x.Email.ToLower() == email).FirstOrDefaultAsync();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching user by email.", ex);   
        }
    }
    
    // DB call to get user by userid
    public User? GetUserById(int userId)
    {
        try
        {
            return _context.Users.FirstOrDefault(x => x.UserId == userId);
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching user by ID.", ex);   
        }
    }

    // DB call to get all user details
    public async Task<List<User>?> GetAllUserDetails()
    {
        try
        {
            return await _context.Users.ToListAsync();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching all user details.", ex);   
        }
    }

    // DB call for adding Reset Password expiry token
    public async Task AddPasswordResetRequest(PasswordResetRequest passwordResetRequest)
    {
        try
        {
            await _context.PasswordResetRequests.AddAsync(passwordResetRequest);
            await _context.SaveChangesAsync();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while adding password reset request.", ex);   
        }
    }

    // DB call for update user
    public void UpdateUser(User user)
    {
        try
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while updating user.", ex);   
        }
    }
    
    // DB call for update password reset token
    public void UpdatePasswordResetRequest(PasswordResetRequest passwordResetRequest)
    {
        try
        {
            _context.PasswordResetRequests.Update(passwordResetRequest);
            _context.SaveChanges();
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while updating password reset request.", ex);   
        }
    }

    // DB call to get password reset request by token
    public PasswordResetRequest? GetPasswordResetRequestByToken(string token)
    {
        try
        {
            return _context.PasswordResetRequests.FirstOrDefault(x => x.Guidtoken == token);
        }
        catch(Exception ex)
        { 
            throw new Exception("An error occurred while fetching password reset request by token.", ex);   
        }
    }
}
