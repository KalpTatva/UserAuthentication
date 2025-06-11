using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserAuth.Repository.interfaces;
using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;
using UserAuth.Service.interfaces;
using static UserAuth.Repository.helpers.Enums;

namespace UserAuth.Service.implementation;

public class UserService : IUserService
{

    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService; 

    public UserService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IEmailService emailService
    )
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;
       
    }

    // Method to handle user login
    public async Task<ResponsesViewModel?> UserLogin(LoginViewModel model)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(model.Email.Trim().ToLower());
    
            if(user!=null && BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password))
            {
                
                // adding otp to the db with expire within 5 minutes

                Random rondom = new Random();
                string randomOTP = rondom.Next(100001,1000000).ToString(); // generates random opt
                string varificationCode = BCrypt.Net.BCrypt.EnhancedHashPassword(randomOTP);

                User2faAuth user2FaAuth = new User2faAuth
                {
                    TokenAuth = varificationCode,
                    Email = model.Email,
                    RememberMe = model.RememberMe,
                    CreateTime = DateTime.Now,
                    ExpireTime = DateTime.Now.AddMinutes(5)
                };
                _userRepository.AddUser2faAuth(user2FaAuth);


                // generating email for user with otp
                string emailBody = 
                    $@"
                    <html>
                    <body>
                        <h1>Varification Code</h1>
                        <p>Dear {user.FirstName},</p>
                        <p>We received a request to login in userAuth</p>
                        <p>Here is your varification code : </p>
                        <h2 style='color:#0565a1;width:100%;display:flex;align-items:center;justify-content:center;'>{randomOTP}</h2>
                        <p>If you encounter any issue or have any question, please do not hesitate to contact our support team.</p>
                            <p><span style='color:#8B8000'>Important note:</span> For security purposes, This will expire in 5 minutes. If you did not request a password reset, please ignore this email or contact our support team immediately.</p>
                        <p>Thank you!</p>
                    </body>
                    </html>
                ";

                await _emailService.SendEmailAsync(
                    model.Email,
                    "OTP for login",
                    emailBody
                );

                return new ResponsesViewModel()
                {
                    IsSuccess = true,
                    Message = "OTP send successfully! please check your email"
                };
            }
            
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = "Invalid User Credentials"
            };

        }
        catch(Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"  
            };
        }
    }

    // 2 factor authentication service
    public ResponseTokenViewModel Validate2faToken(User2FAViewModel model)
    {
        try
        {
            User2faAuth user2FaAuth = _userRepository.GetUser2faAuth(model.ToEmail.Trim());
            User? user = _userRepository.GetUserByEmail(model.ToEmail.Trim().ToLower());

            if(user2FaAuth != null)
            {
                // delete the token (tokens) if time is expired 
                if(user2FaAuth.ExpireTime < DateTime.Now || user2FaAuth.Counting >=3)
                {
                    _userRepository.DeleteALlAuthTokenByEmail(model.ToEmail.Trim().ToLower());
                    return new ResponseTokenViewModel()
                    {
                        token = "",
                        response = "Token expired",
                    };
                }
                else
                {
                    if(BCrypt.Net.BCrypt.EnhancedVerify(model.Token, user2FaAuth.TokenAuth))
                    {
                        string? roleName = user != null && user.Role != 0 ? ((UserRole)user.Role).ToString() : null;

                        var TokenExpireTime = user2FaAuth.RememberMe == true
                                ? DateTime.Now.AddDays(30)
                                : DateTime.Now.AddDays(1);
                            var token = GenerateJwtToken(model.ToEmail, TokenExpireTime, roleName ?? "");
                            
                            // deleting all tokens which are generated for this email
                            _userRepository.DeleteALlAuthTokenByEmail(model.ToEmail.Trim().ToLower());
                            
                            if (token != null)
                            {
                                return new ResponseTokenViewModel()
                                {
                                    token = token,
                                    isPersistent = user2FaAuth.RememberMe ?? false,
                                    response = "Login successful",
                                };
                            }
                    }else{
                        // update the count of the token 
                        // only 3 tries are allowed
                        user2FaAuth.Counting += 1;
                        _userRepository.UpdateUser2faAuth(user2FaAuth);
                        return new ResponseTokenViewModel()
                        {
                            token = "",
                            response = $"Invalid User Credentials! only {3-user2FaAuth.Counting} are left",
                        }; 
                    }
                }
            }
            return new ResponseTokenViewModel() { token = "", response = "Token expired" };
        }
        catch(Exception ex)
        {
           throw new Exception($"Error in LoginService: {ex.Message}"); 
        }
    }

    // Method to generate JWT token
    private string GenerateJwtToken(string email, DateTime expiryTime, string? RoleName)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, RoleName), 
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiryTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Method for forget password (resetting the password)
    public async Task<ResponsesViewModel> ForgetPassword(EmailViewModel email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email.ToEmail.Trim().ToLower());
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }

            // main logic for forget password
            PasswordResetRequest passwordResetRequest = new PasswordResetRequest()
            {
                Userid = user.UserId,
                Createdate = DateTime.Now,
                Guidtoken = Guid.NewGuid().ToString()
            };
            await _userRepository.AddPasswordResetRequest(passwordResetRequest);
            
            // reset password url for email
            string BaseUrl = _configuration["UrlSettings:BaseUrl"] ?? "";
            string ResetPasswordUrl = $"{BaseUrl}/Home/ResetPassword?token={passwordResetRequest.Guidtoken}";
            string ResetLink = HtmlEncoder.Default.Encode(ResetPasswordUrl);

            string EmailBody = $@"
                                <html>
                                <body>
                                    <h1>Password Reset Request</h1>
                                    <p>Dear {user.FirstName},</p>
                                    <p>We received a request to reset your password. Please click the link below to reset your password:</p>
                                    <a href='{ResetLink}' style='color:blue;'>Reset Password</a>
                                    <p style='color:red;'>This link will expire in 1 hour.</p>
                                    <p>If you did not request this change, please ignore this email.</p>
                                    <p>Thank you!</p>
                                </body>
                                </html>";
            
            await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Request",
                    EmailBody
                );
            
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "Reset password email sent successfully, please check your emails" // when reset password email is sent
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    // cheking for reset password token
    public ResponsesViewModel ValidateResetPasswordToken(string token)
    {
        try
        {
            // check if token is null or empty
            if (string.IsNullOrEmpty(token))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid password reset link" // when token is null or empty
                };
            }

            PasswordResetRequest? resetRequest = _userRepository.GetPasswordResetRequestByToken(token);
            // check if token is not found in the database
            if (resetRequest == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid password reset link" // when token is not found
                };
            }

            // check if token is closed or expired
            if (resetRequest.Closedate != null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Password reset link has expired" // when token is expired
                };
            }

            // check if token is created more than 1 hour ago
            if (resetRequest.Createdate < DateTime.Now.AddHours(-1))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Password reset link has expired" // when token is expired
                };
            }
            
            // check if user exists for the token
            User? user = _userRepository.GetUserById(resetRequest.Userid);
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }

            // if all checks are passed, return success
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = user.Email // when token is valid
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    // Method to reset the password
    public ResponsesViewModel ResetPassword(ForgetPasswordViewModel model)
    {
        try
        {
            // check if model is null or empty
            if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid request" // when model is null or empty
                };
            }

            // validate the reset password token
            ResponsesViewModel response = ValidateResetPasswordToken(model.Token);
            if (!response.IsSuccess)
            {
                return response; // return error response if token is invalid
            }

            // get user by email
            User? user = _userRepository.GetUserByEmail(model.Email.Trim().ToLower());
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }
            // check if password is same as old password
            if (BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "New password cannot be same as old password" // when new password is same as old password
                };
            }

            // update user password
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password);
            _userRepository.UpdateUser(user);

            // close the reset password request
            PasswordResetRequest? resetRequest = _userRepository.GetPasswordResetRequestByToken(model.Token);
            if (resetRequest != null)
            {
                resetRequest.Closedate = DateTime.Now;
                _userRepository.UpdatePasswordResetRequest(resetRequest);
            }

            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "Password reset successfully" // when password is reset successfully
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }


    // Method to get all user details
    public async Task<List<User>?> GetAllUserDetails()
    {
        try
        {
            List<User>? userDetails = await _userRepository.GetAllUserDetails();
            return userDetails;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error 500 : Internal Server Error {ex.Message}");
        }
    }


    // Method to register a new user
    public async Task<ResponsesViewModel> RegisterUser(RegisterUserViewModel model)
    {
        try
        {
            // check if model is null or empty
            if (model == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid request" // when model is null or empty
                };
            }

            // check if user already exists
            User? existingUser = _userRepository.GetUserByEmail(model.Email.Trim().ToLower());
            if (existingUser != null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User already exists" // when user already exists
                };
            }

            // check if user is +18 years old
            if (model.DateOfBirth > DateTime.Now.AddYears(-18))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User must be at least 18 years old" // when user is not 18 years old
                };
            }

            // create new user
            model.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password);
            model.Email = model.Email.Trim().ToLower();
            bool response = await _userRepository.RegisterUser(model);
           
            if (!response)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User registration failed" // when user registration fails
                };
            }
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "User registered successfully" // when user is registered successfully
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    // edit users
    public User GetUserById(int userId)
    {
        try
        {
            User? user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error 500 : Internal Server Error {ex.Message}");
        }
    }

    // delete user (soft delete)
    public ResponsesViewModel DeleteUser(int userId, string email)
    {
        try
        {
            User? user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }

            if(user.Role == (int)UserRole.Admin)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Admin user cannot be deleted" // when trying to delete admin user
                };
            }

            // soft delete user using sp
            User? user1 = _userRepository.GetUserByEmail(email.Trim().ToLower());
            bool res = _userRepository.DeleteUser(userId, user1.UserId);
            if (!res)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User deletion failed" // when user deletion fails
                };
            }

            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "User deleted successfully" // when user is deleted successfully
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    // Method to update user details
    public ResponsesViewModel UpdateUser(User user, string email)
    {
        try
        {
            // check if user is null
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid request" // when user is null
                };
            }

            // check if user exists
            User? existingUser = _userRepository.GetUserByEmail(user.Email.Trim().ToLower());
            if (existingUser == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }

            // check if user is +18 years old
            if (user.DateOfBirth > DateTime.Now.AddYears(-18))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User must be at least 18 years old" // when user is not 18 years old
                };
            }

            // update user stored procedure
            User? user1 = _userRepository.GetUserByEmail(email.Trim().ToLower());
            bool res = _userRepository.UpdateUserSP(user, user1.UserId);
            if (!res)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User update failed" // when user update fails
                };
            }
            
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "User updated successfully" // when user is updated successfully
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    // log histories
    public List<UsersHistory> LogUserHistory()
    {
        try{

            List<UsersHistory> usersHistories = _userRepository.GetUserHistroryLog();
            return usersHistories;

        }catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    // method for deleting all auth token which are generated for 2 factor authentication
    public bool Delete2FaAuth(string Email)
    {
        try
        {
            _userRepository.DeleteALlAuthTokenByEmail(Email.Trim().ToLower());
            return true;
        }
        catch
        {
            return false;
        }
    }

}
