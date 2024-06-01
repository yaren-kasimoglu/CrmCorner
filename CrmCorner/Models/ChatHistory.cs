using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class ChatHistory
{
    public int Id { get; set; }

    public string SenderId { get; set; } 

    public string ReceiverId { get; set; } 

    public string Message { get; set; }

    public DateTime MessageTime { get; set; }

    public bool IsRead { get; set; }

}
