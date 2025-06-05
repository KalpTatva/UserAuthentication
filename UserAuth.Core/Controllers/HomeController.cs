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
                TempData["error"] = "Invalid user credentials";
                return View(model);
            }
            ResponseTokenViewModel? response = await _userService.UserLogin(model);
            if (response != null && response?.token?.Length > 0)
            {
                // setting jwt cookie 
                CookieUtils.SetJwtCookie(Response, response.token, response.isPersistent);
                
                TempData["success"] = "User logged in successfully!";
                if(response.Role == UserRole.Admin.ToString())
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if(response.Role == UserRole.User.ToString())
                {
                    return RedirectToAction("IndexOfUser", "Dashboard");
                }else {
                    TempData["error"] = "Invalid user role. Please contact support.";
                    return View(model);
                }
            }
            res = response != null ? response.response ?? "" : "";
            TempData["Error"] = res;
            return View(model);
        }
        catch(Exception ex)
        {
            TempData["error"] = ex.Message;
            return View(model);
        }
    }


    // method for logout
    public IActionResult Logout()
    {
        CookieUtils.ClearCookies(Response);
        return RedirectToAction("Index", "Home");
    }



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
                return View();
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
                return View(model);
            }


        }catch(Exception ex)
        {
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
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return RedirectToAction("ForgetPassword");
        }
    }


    [HttpPost]
    public async Task<IActionResult> ResetPassword(ForgetPasswordViewModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            TempData["ErrorMessage"] = "All fields are required.";
            return View(model);
        }
        try
        {
            ResponsesViewModel response = await _userService.ResetPassword(model);
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
            TempData["ErrorMessage"] = $"Error occurred while processing your request: {ex.Message}";
            return View(model);
        }
    }



    // error views
    public IActionResult Error403()
    {
        return View();
    }
    public IActionResult Error404()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
