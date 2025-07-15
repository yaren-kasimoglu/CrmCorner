using CrmCorner.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models;

public partial class CustomerN
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? CustomerEmail { get; set; } = null!;
    public string? CustomerTitle { get; set; }

    [RegularExpression(@"^\d{10}$", ErrorMessage = "Lütfen 10 haneli bir telefon numarası giriniz.")]
    public string? PhoneNumber { get; set; } = null!;

    public string? CompanyName { get; set; } = null!;

    public string? CompanyEmail { get; set; } = null!;

    public string? LinkedinUrl { get; set; }


    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    [Display(Name = "Çalışan Sayısı")]
    public EmployeeCountRange? EmployeeCount { get; set; }

    public string? Source { get; set; }


    public string? AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    public IndustryType? Industry { get; set; } // Sektör bilgisi

    public virtual ICollection<TaskComp> Taskcomps { get; set; } = new List<TaskComp>();
}
