using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmCorner.Models;

public  class Calendar
{
    public int Id { get; set; }


    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Date { get; set; } = null!;
    [NotMapped]
    public EmailProperty Email { get; set; }

}
public class EmailProperty
{
    [NotMapped]
    public string Email { get; set; }
    // Diğer özellikler...
}




