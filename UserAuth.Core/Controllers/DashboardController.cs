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
    public DashboardController(IUserService userService)
    {
        _userService = userService;
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
                    DateOfBirth = user.DateOfBirth
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
    [HttpPut]
    public IActionResult EditUser(User user)
    {
        try
        {
            if (user != null)
            {
                ResponsesViewModel response = _userService.UpdateUser(user);
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
    [HttpPut]
    public IActionResult DeleteUser(int userId)
    {
        try
        {
            ResponsesViewModel response = _userService.DeleteUser(userId);
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
    #endregion
    #region User


    // Index method for User
    [Authorize(Roles = "User")]
    public ActionResult IndexOfUser()
    {
        return View();
    }


    #endregion

}

internal class httpPostAttribute : Attribute
{
}