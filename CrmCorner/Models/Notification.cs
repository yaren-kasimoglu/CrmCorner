using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime DateCreated { get; set; }

    public string? AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    public int? TaskCompId { get; set; }
    public virtual TaskComp? TaskComp { get; set; }
}
