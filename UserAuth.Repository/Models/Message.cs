using System;
using System.Collections.Generic;

namespace UserAuth.Repository.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int SenderId { get; set; }

    public int ReciverId { get; set; }

    public string MessagePayload { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
