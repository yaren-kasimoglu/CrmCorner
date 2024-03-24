using System;
namespace CrmCorner.Models
{
	public class ToDoList
	{
        public ToDoList()
        {
        }

        public int Id { get; set; }

        public string? Title { get; set; }

        public string? MainGoalTitle { get; set; }

        public string? DoneList { get; set; }

        public string? NotDoneList { get; set; }


        public DateTime SystemDate { get; set; }

        public string? UserId { get; set; }

        //public virtual AppUser? AppUser { get; set; }

    }
}


