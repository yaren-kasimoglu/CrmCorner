using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Department
{
    public int IdDepartment { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? DepartmentDescription { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
