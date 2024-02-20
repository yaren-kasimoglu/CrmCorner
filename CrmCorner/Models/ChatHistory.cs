using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class ChatHistory
{
    public string Id { get; set; } = null!;

    public string SenderId { get; set; } = null!;

    public string ReceiverId { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime MessageTime { get; set; }
}
