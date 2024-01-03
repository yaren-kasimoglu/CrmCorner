using System;
namespace CrmCorner.Models
{
	public partial class ChatHistory
	{
       
            public string Id { get; set; }

            public string SenderId { get; set; }

            public string ReceiverId { get; set; }

            public string Message { get; set; }

            public DateTime MessageTime { get; set; }

            //public ChatHistory()
            //{
            //    Id = Id.GenerateNewId();
            //}
        
    }
}

