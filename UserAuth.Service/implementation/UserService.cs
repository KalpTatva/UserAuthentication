using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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
    public async Task<ResponseTokenViewModel> UserLogin(LoginViewModel model)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(model.Email.Trim());
    
            if(user!=null && BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password))
            {
                string? RoleName = user.Role != 0 ? ((UserRole)user.Role).ToString():null;
                
                DateTime expiryTime = model.RememberMe 
                    ? DateTime.Now.AddDays(30) 
                    : DateTime.Now.AddMinutes(60);

                string token = GenerateJwtToken(model.Email, expiryTime, RoleName);
                if (token != null)
                {
                    return new ResponseTokenViewModel()
                    {
                        token = token,
                        response = "Login successful",
                        isPersistent = model.RememberMe,
                        Role = RoleName
                    };
                }
            }
            
            return new ResponseTokenViewModel()
            {
                token = "",
                response = "Invalid User Credentials",
                isPersistent = model.RememberMe
            };

        }
        catch(Exception ex)
        {
            return new ResponseTokenViewModel()
            {
                token = "",
                response = $"Error 500 : Internal Server Error {ex.Message}",
                isPersistent = false
            };
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
            User? user = _userRepository.GetUserByEmail(email.ToEmail.Trim());
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
            User? user = _userRepository.GetUserByEmail(model.Email.Trim());
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

}
