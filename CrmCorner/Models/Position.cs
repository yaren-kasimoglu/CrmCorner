using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Position
{
    public int IdPositions { get; set; }

    public string PositionName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
