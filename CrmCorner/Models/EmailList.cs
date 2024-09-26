using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models
{
	public class EmailList
	{
		

        public int Id { get; set; }

        public string? Subject { get; set; }

        public string? Body { get; set; }
        public string? SendMail { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Boolean? IsStar { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? CC { get; set; }
        public string? AppUserId { get; set; }
        [NotMapped]
        public Boolean IsSend { get; set; }

    }
}

