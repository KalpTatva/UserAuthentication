using System;
using System.Collections.Generic;

namespace UserAuth.Repository.Models;

public partial class User2faAuth
{
    public int Userid { get; set; }

    public string TokenAuth { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool? RememberMe { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime ExpireTime { get; set; }

    public int? Counting { get; set; }
}
