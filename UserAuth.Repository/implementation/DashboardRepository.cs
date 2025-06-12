using System.Data;
using UserAuth.Repository.interfaces;
using UserAuth.Repository.Models;

namespace UserAuth.Repository.implementation;

public class DashboardRepository : IDashboardRepository
{
    private readonly UserAuthContext _context;
    private IDbConnection _dbConnection { get; }
    public DashboardRepository(UserAuthContext context, IDbConnection dbConnection)
    {
        _context = context;
        _dbConnection = dbConnection;

    }

    public void SaveMessage(Message message)
    {
        try{
            _context.Messages.Add(message);
            _context.SaveChanges();
        }catch(Exception ex)
        {
            throw new Exception("An error occurred while adding message. ", ex);   
        }
    }

    public List<Message> GetMessagesForAdminByReciverId(int ReciverId, int SenderId)
    {
        try
        {
            List<Message>? messages = _context.Messages.Where( x =>
                (x.ReciverId == ReciverId && x.SenderId == SenderId) ||
                (x.ReciverId == SenderId && x.SenderId == ReciverId)
            )
            .OrderBy(x => x.CreatedAt)
            .ToList();

            return messages ?? new List<Message>();
        }
        catch(Exception ex)
        {
            throw new Exception("An error occurred while reciving message from db. ", ex);   
        }
    }
}
