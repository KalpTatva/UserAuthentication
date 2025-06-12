using UserAuth.Repository.Models;

namespace UserAuth.Repository.interfaces;

public interface IDashboardRepository
{
    void SaveMessage(Message message);
    List<Message> GetMessagesForAdminByReciverId(int ReciverId, int SenderId);
}
