using UserAuth.Repository.Models;

namespace UserAuth.Repository.ViewModels;

public class MessageViewModel
{
    public User? user {get; set;}
    public string? Message {get;set;}
    public int sendTo {get;set;}
}
