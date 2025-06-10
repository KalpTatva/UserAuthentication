using System;
using System.Collections.Generic;

namespace UserAuth.Repository.Models;

public partial class UsersHistory
{
    public int HistoryId { get; set; }

    public string? History { get; set; }

    public int? Operation { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
