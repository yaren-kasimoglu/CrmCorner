using System;
using System.Collections.Generic;
using CrmCorner.Models;

namespace CrmCorner.ViewModels
{
    public class ToDoPageViewModel
    {
        public int CurrentBoardId { get; set; }
        public string Title { get; set; }

        public List<ToDoBoardViewModel> Boards { get; set; } = new();
        public List<TodoEntry> DoneItems { get; set; } = new();
        public List<TodoEntry> NotDoneItems { get; set; } = new();

        public int ImportantId { get; set; }
        public int AssignListId { get; set; }
    }
}
