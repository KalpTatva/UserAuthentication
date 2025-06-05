namespace UserAuth.Repository.ViewModels;


// class for getting response token for login perpose (used in UserService.cs)
public class ResponseTokenViewModel
{
    public string? token { get; set; }
    public string? response { get; set;}
    public bool isPersistent { get; set;} = false;
    public string? Role { get; set; } 
}

public class ResponsesViewModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}
