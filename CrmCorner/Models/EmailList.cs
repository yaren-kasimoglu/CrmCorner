using System;
namespace CrmCorner.Models
{
	public class EmailList
	{
		

        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        public string SendMail { get; set; }
        public DateTime CreatedDate { get; set; }
        public Boolean IsStar { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }

    }
}

