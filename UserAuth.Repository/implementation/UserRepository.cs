using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using UserAuth.Repository.interfaces;
using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;

namespace UserAuth.Repository.implementation;

public class UserRepository: IUserRepository
{

    private readonly UserAuthContext _context;
    private IDbConnection _dbConnection { get; }
    public UserRepository(UserAuthContext context, IDbConnection dbConnection)
    {
        _context = context;
        _dbConnection = dbConnection;

    }

    // DB call to get user by email
    public User? GetUserByEmail(string email)
    {
        try
        {
            return _context.Users.Where(x => x.Email.ToLower() == email && x.IsDeleted == false).FirstOrDefault();
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
            return _context.Users.FirstOrDefault(x => x.UserId == userId && x.IsDeleted == false);
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
            return await _context.Users.Where(x => x.IsDeleted == false).OrderBy(x => x.UserId).ToListAsync();
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

    // DB call for user log history
    public List<UsersHistory> GetUserHistroryLog()
    {
        try{
            List<UsersHistory> usersHistories = _context.UsersHistories.ToList();
            return usersHistories;
        }catch(Exception ex){
            throw new Exception(ex.Message);
        }
    }



    // DB call for Registering a new user using stored procedure
    public async Task<bool> RegisterUser(RegisterUserViewModel model)
    {
        string query = "CALL SP_RegisterUser(@in_UserName::varchar, @in_Email::varchar, @in_PhoneNumber::varchar, @in_FirstName::varchar, @in_LastName::varchar, @in_Password::varchar, @in_Role::integer, @in_DateOfBirth::timestamp, @in_Address::varchar, @in_Pincode::integer)";
        var parameters = new DynamicParameters();
        parameters.Add("in_UserName", model.UserName, DbType.String, ParameterDirection.Input);
        parameters.Add("in_Email", model.Email, DbType.String, ParameterDirection.Input);
        parameters.Add("in_PhoneNumber", model.PhoneNumber, DbType.String, ParameterDirection.Input);
        parameters.Add("in_FirstName", model.FirstName, DbType.String, ParameterDirection.Input);
        parameters.Add("in_LastName", model.LastName, DbType.String, ParameterDirection.Input);
        parameters.Add("in_Password", model.Password, DbType.String, ParameterDirection.Input);
        parameters.Add("in_Role", model.Role, DbType.Int32, ParameterDirection.Input);
        parameters.Add("in_DateOfBirth", model.DateOfBirth, DbType.Date, ParameterDirection.Input);
        parameters.Add("in_Address", model.Address, DbType.String, ParameterDirection.Input);
        parameters.Add("in_Pincode", model.Pincode, DbType.Int32, ParameterDirection.Input);

        
        try
        {
            int result = await _dbConnection.ExecuteAsync(query, parameters);
            return true; // Returns true if at least one row was affected
        }
        catch(Exception ex)
        {
            // throw new Exception("An error occurred while registering the user.", ex);
            System.Console.WriteLine($"An error occurred while registering the user: {ex.Message}");
            return false; 
        }       
    }

    // DB call for soft delete user using stored procedure (soft delete)
    public bool DeleteUser(int userId, int deletedByUserId)
    {
        string query = "CALL sp_deleteuser(@in_UserId::integer,@in_deletedByUserId::integer)";
        var parameters = new DynamicParameters();
        parameters.Add("in_UserId", userId, DbType.Int32, ParameterDirection.Input);
        parameters.Add("in_deletedByUserId", deletedByUserId, DbType.Int32, ParameterDirection.Input);
        try
        {
            int result = _dbConnection.Execute(query, parameters);
            return true;
        }
        catch(Exception ex)
        {
            // throw new Exception("An error occurred while deleting the user.", ex);
            System.Console.WriteLine($"An error occurred while deleting the user: {ex.Message}");
            return false; 

        }
    }

    // DB call for updating user details using stored procedure
    public bool UpdateUserSP(User user,int EditedByuserid)
    {
        string query = "CALL SP_UpdateUser(@in_UserName::varchar, @in_Email::varchar, @in_PhoneNumber::varchar, @in_FirstName::varchar, @in_LastName::varchar, @in_DateOfBirth::timestamp, @in_Address::varchar, @in_EditedByuserid::integer, @in_Pincode::integer)";
        var parameters = new DynamicParameters();
        parameters.Add("in_UserName", user.UserName, DbType.String, ParameterDirection.Input);
        parameters.Add("in_Email", user.Email, DbType.String, ParameterDirection.Input);
        parameters.Add("in_PhoneNumber", user.PhoneNumber, DbType.String, ParameterDirection.Input);
        parameters.Add("in_FirstName", user.FirstName, DbType.String, ParameterDirection.Input);
        parameters.Add("in_LastName", user.LastName, DbType.String, ParameterDirection.Input);
        parameters.Add("in_DateOfBirth", user.DateOfBirth, DbType.Date, ParameterDirection.Input);
        parameters.Add("in_Address", user.Address, DbType.String, ParameterDirection.Input);
        parameters.Add("in_EditedByuserid", EditedByuserid, DbType.Int32, ParameterDirection.Input);
        parameters.Add("in_Pincode", user.Pincode, DbType.Int32, ParameterDirection.Input);

        try
        {
            int result = _dbConnection.Execute(query, parameters);
            return true;
        }
        catch(Exception ex)
        {
            // throw new Exception("An error occurred while updating the user.", ex);
            System.Console.WriteLine($"An error occurred while updating the user: {ex.Message}");
            return false;
        }
    }
}

