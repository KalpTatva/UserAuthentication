using Microsoft.Extensions.Configuration;
using UserAuth.Repository.interfaces;
using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;
using UserAuth.Service.interfaces;

namespace UserAuth.Service.implementation;

public class DashBoardSerivice : IDashBoardSerivice
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IEmailService _emailService; 

    public DashBoardSerivice(
        IUserRepository userRepository,
        IConfiguration configuration,
        IEmailService emailService,
        IDashboardRepository dashboardRepository
    )
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;
        _dashboardRepository = dashboardRepository;
       
    }


    public ResponsesViewModel SendMessage(MessageViewModel model, string email)
    {
        try{
            if(model!=null)
            {
                User? user = _userRepository.GetUserByEmail(email.Trim().ToLower());
                Message message = new (){
                    CreatedAt = DateTime.Now,
                    SenderId = user.UserId,
                    ReciverId = model.sendTo,
                    MessagePayload = model.Message ?? "",
                };

                _dashboardRepository.SaveMessage(message);
                
                return new ResponsesViewModel()
                {
                    IsSuccess = true,
                    Message = "Message sent successfully" // when user update fails
                };
            }
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = "Enter valid details" 
            };
        }catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    public List<Message>? ReciveMessageAtAdmin(int messageTo, string email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email.Trim().ToLower());
            List<Message>? messages = _dashboardRepository.GetMessagesForAdminByReciverId(messageTo, user.UserId);
           
            return messages ?? new List<Message>();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
