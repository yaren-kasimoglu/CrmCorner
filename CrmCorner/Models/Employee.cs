using System;
using System.Collections.Generic;

namespace CrmCorner.Models;

public partial class Employee
{
    public int IdEmployee { get; set; }

    public string EmployeeName { get; set; } = null!;

    public string EmployeeSurname { get; set; } = null!;

    public string EmployeeEmail { get; set; } = null!;

    public string EmployeePhone { get; set; } = null!;

    public int? IdDepartment { get; set; }

    public int? IdPositions { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual Department? IdDepartmentNavigation { get; set; }

    public virtual Position? IdPositionsNavigation { get; set; }

    public virtual ICollection<TaskComp> TaskComps { get; set; } = new List<TaskComp>();
}
