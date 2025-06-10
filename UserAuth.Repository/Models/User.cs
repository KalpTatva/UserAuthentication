using System;
using System.Collections.Generic;

namespace UserAuth.Repository.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public DateTime DateOfBirth { get; set; }

    public int Age { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime EditedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string? Address { get; set; }

    public int? EditedById { get; set; }

    public int? DeletedById { get; set; }

    public int? Pincode { get; set; }
}
