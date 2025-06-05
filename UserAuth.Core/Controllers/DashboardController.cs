using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
