using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAuth.Core.Filters;
using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;
using UserAuth.Service.interfaces;

namespace UserAuth.Core.Controllers;

public class DashboardController : Controller
{
    private readonly IUserService _userService;
    private readonly IDashBoardSerivice _dashboardService;
    public DashboardController(IUserService userService, IDashBoardSerivice dashboardService)
    {
        _userService = userService;
        _dashboardService = dashboardService;
    }

    #region Admin


    // Index method for Admin
    [Authorize(Roles = "Admin")]
    public ActionResult Index()
    {
        return View();
    }

    // Method to get user details 

    // custom authorization filter
    // [CustomAuthorizationFilter("User","Admin")]
    // [CustomAuthorizationFilter("Admin")]
    
    // role based authorization
    // [Authorize(Roles ="User,Admin")]
    // [Authorize(Roles ="Admin")]

    // policy based authorization
    // [Authorize(Policy = "PublicAccess")]
    // [Authorize(Policy = "AdminOnly")]
    // [Authorize(Policy = "UserOnly")]
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUserDetails()
    {
        try{
            List<User>? userDetails = await _userService.GetAllUserDetails();
            if(userDetails != null && userDetails.Count > 0)
            {
                return PartialView("_UserDetailsPartial", userDetails);
            }
            else
            {
                return Json(new { success = false, message = "No user details found." });
            }
       
        }catch(Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Method for edit user details (get user details)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult EditUser(int userId)
    {
        try
        {
            User? user = _userService.GetUserById(userId);
            if (user != null)
            {
                EditUserViewModel editUserViewModel = new EditUserViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Age = user.Age,
                    Address = user.Address,
                    Pincode = user.Pincode ?? 0
                };
                return PartialView("_EditUserPartial", editUserViewModel);
            }
            else
            {
                return Json(new { success = false, message = "User not found." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    
    // Method for updating user details (update details)
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public IActionResult EditUser(User user)
    {
        try
        {   
            string? email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (user != null)
            {
                ResponsesViewModel response = _userService.UpdateUser(user, email);
                if (response.IsSuccess)
                {
                    return Json(new { success = true, message = "User updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = "Invalid user data." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


    // Method for deleting user (soft delete)
    [Authorize(Roles = "Admin")]
    [HttpPut]
    public IActionResult DeleteUser(int userId)
    {
        try
        {
            string? email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            ResponsesViewModel response = _userService.DeleteUser(userId, email);
            if (response.IsSuccess)
            {
                return Json(new { success = true, message = "User deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = response.Message });
            }
            
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    public IActionResult UserDetails()
    {
        return View();
    }
    #endregion
    #region User


    // Index method for User
    [Authorize(Roles = "User")]
    public ActionResult IndexOfUser()
    {
        return View();
    }


    #endregion
    [Authorize(Roles = "Admin")]
    public IActionResult Logs()
    {
        return View();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult LogsDetails()
    {
        try
        {
            List<UsersHistory> usersHistories = _userService.LogUserHistory();
            if(usersHistories != null && usersHistories.Any())
            {
                return PartialView("_LogsPartial" , usersHistories);
            }
            else 
            {
                return Json(new { success = false, message = "Error occured while fetching log information " });
            }
        }catch(Exception ex){
            return Json(new { success = false, message = ex.Message });
        }
    }


    public IActionResult Message(int messageTo)
    {   
        try{
            
            User? user = _userService.GetUserById(messageTo);
            MessageViewModel message = new () {
                user = user,
                sendTo = messageTo
            };
            return View(message);

            
        }catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    [HttpPost]
    public IActionResult SendMessage(MessageViewModel model)
    {
        try
        {
            string? email =  User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            ResponsesViewModel res = _dashboardService.SendMessage(model, email);
            if(res.IsSuccess)
            {
                return Json(new {success = true, message = res.Message});
            }
            return Json(new {success = false, message = res.Message});
        }catch(Exception e)
        {
            return Json(new {success = false, message = $"{e.Message}"});
        }
    }

    // [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult ReciveMessageAdmin(int messageTo)
    {
        try
        {
            string? email =  User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            List<Message>? messages = _dashboardService.ReciveMessageAtAdmin(messageTo, email ?? "");
            if(messages != null && messages.Any())
            {
                return PartialView("_MessagesPartial",messages);
            }
            return Json(new {success = false, message = "Error occured while fetching messages"});
        }
        catch(Exception e)
        {
            return Json(new {success = false, message = $"{e.Message}"});
        }
    }

}

