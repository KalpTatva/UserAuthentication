using UserAuth.Repository.Models;
using UserAuth.Repository.ViewModels;

namespace UserAuth.Service.interfaces;

public interface IDashBoardSerivice
{
    
    List<Message>? ReciveMessageAtAdmin(int messageTo, string email);
    ResponsesViewModel SendMessage(MessageViewModel model, string email);
}
