using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Calendar
{

    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Date { get; set; } = null!;
}
