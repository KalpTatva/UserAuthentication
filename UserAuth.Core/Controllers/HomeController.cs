using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserAuth.Core.Models;
using UserAuth.Core.Utils;
using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;
using UserAuth.Service.interfaces;
using static UserAuth.Repository.helpers.Enums;

namespace UserAuth.Core.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    #region login
    // index method for redirection based on user role
    public IActionResult Index()
    {
        if (Request.Cookies.ContainsKey("auth_token") && User?.Identity?.IsAuthenticated == true)
        {
            // getting user role from claims
            string? role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role == UserRole.Admin.ToString())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (role == UserRole.User.ToString())
            {
                return RedirectToAction("IndexOfUser", "Dashboard");
            }
            else
            {
                TempData["error"] = "Invalid user role. Please contact support.";
                return View();
            }
        }
        else
        {
            _logger.LogInformation("User is not authenticated, redirecting to login page.");
            return View();
        }
    }

    // post method for login
    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        string res = "";
        try{
            if(!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid user credentials";
                return View(model);
            }
            ResponsesViewModel? response = await _userService.UserLogin(model);
            if(response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("User2FaAuth", new {Email = model.Email});
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user login.");
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }

    
    [HttpGet]
    public IActionResult User2FaAuth(string Email)
    {
        if (!SessionUtils.IsTimerAvailable(HttpContext))
        {
            SessionUtils.SetTimer(HttpContext, DateTime.UtcNow.ToString());
        }

        // utility function which find the remaining time of user
        // for 2 FA authentication
        ViewBag.CountdownTime = SessionUtils.GetRemainingTimer(HttpContext);

        User2FAViewModel emailViewModel = new User2FAViewModel();
    
        emailViewModel.ToEmail = Email;

        return View(emailViewModel);
    }

    [HttpPost]
    public IActionResult User2faAuth(User2FAViewModel model)
    {
        try
        {
            if(ModelState.IsValid)
            {
                ResponseTokenViewModel? isValidToken = _userService.Validate2faToken(model);
                if (isValidToken != null && isValidToken?.token?.Length > 0)
                {
                    CookieUtils.SetJwtCookie(Response, isValidToken.token, isValidToken.isPersistent);
                    SessionUtils.RemoveTimer(HttpContext);
                    
                    TempData["success"] = "logged in";
                    return RedirectToAction("Index", "Home");
                }
                if(isValidToken != null && isValidToken.response != "Login successful")
                {
                    if(isValidToken.response == "Token expired")
                    {
                        SessionUtils.RemoveTimer(HttpContext);
                        
                        TempData["SuccessMessage"] = "Login successfully!";
                        return RedirectToAction("Index");
                    }

                    if (!SessionUtils.IsTimerAvailable(HttpContext))
                    {
                        SessionUtils.SetTimer(HttpContext, DateTime.UtcNow.ToString());
                    }

                    // utility function which find the remaining time of user
                    // for 2 FA authentication
                    ViewBag.CountdownTime = SessionUtils.GetRemainingTimer(HttpContext);

                    TempData["ErrorMessage"] = isValidToken.response;
                    return View(model);
                }
                TempData["ErrorMessage"] = "Invalid code! Try again";
            }
            return View(model);

        }
        catch(Exception ex)
        {
            TempData["ErrorMessage"] = $"authentication failed. {ex.Message}";
            return View(model);
        }
    }

    // method for logout
    public IActionResult Logout()
    {
        CookieUtils.ClearCookies(Response);
        _logger.LogInformation("User logged out successfully.");
        return RedirectToAction("Index", "Home");
    }

    // method for deleting all the token which are generated for 
    // 2FA login but not used
    [HttpPost]
    public IActionResult Delete2FaAuth(string Email)
    {
        try{
            bool response = _userService.Delete2FaAuth(Email);
            if(response)
            {
                return Json(new { success = true, message = "Time out! please try again" });
            }

            return Json(new { success = false, message = "Error while user authentication!" });

        }catch(Exception ex){
            return Json(new { success = false, message = $"Error while user authentication! : {ex.Message}" });
        }
    }

    #endregion
    #region reset password


    // forget password ( restting the password )
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(EmailViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.ToEmail))
        {
            TempData["ErrorMessage"] = "Email address is required.";
        }
        try{
            
            ResponsesViewModel response = await _userService.ForgetPassword(model);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index","Home");
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }


        }catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the forget password request.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return View(model);
        }
    }

    
    // method for validating the reset password token
    [HttpGet]
    public ActionResult ResetPassword(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("ForgetPassword");
            }
            ResponsesViewModel response = _userService.ValidateResetPasswordToken(token);
            if (response.IsSuccess)
            {
                return View(new ForgetPasswordViewModel { Token = token, Email = response.Message });
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                TempData.Remove("SuccessMessage");
                return RedirectToAction("ForgetPassword");
            }

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while validating the reset password token.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return RedirectToAction("ForgetPassword");
        }
    }


    [HttpPost]
    public IActionResult ResetPassword(ForgetPasswordViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            TempData["ErrorMessage"] = "All fields are required.";
            return View(model);
        }
        try
        {
            ResponsesViewModel response = _userService.ResetPassword(model);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while resetting the password.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return View(model);
        }
    }

    #endregion

    #region Register

    // method for user registration
    [HttpGet]
    public IActionResult RegisterUser()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
    {
        if (model == null || !ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid user registration details.";
            return View(model);
        }
        try
        {
            ResponsesViewModel response = await _userService.RegisterUser(model);
            if (response.IsSuccess)
            {
                TempData["SuccessMessage"] = response.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }
            
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user registration.");
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return View(model);
        }
    }

    #endregion

    // error views
    public IActionResult Error403()
    {
        _logger.LogWarning("403 Forbidden error occurred.");
        return View();
    }
    public IActionResult Error404()
    {
        _logger.LogWarning("404 Not Found error occurred.");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
