using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models.Enums
{
    public enum EmployeeCountRange
    {
        [Display(Name = "1-10")]
        OneToTen = 1,
        [Display(Name = "11-50")]
        ElevenToFifty,
        [Display(Name = "51-100")]
        FiftyOneToOneHundred,
        [Display(Name = "101-500")]
        OneHundredOneToFiveHundred,
        [Display(Name = "501-1000")]
        FiveHundredOneToOneThousand,
        [Display(Name = "1000+")]
        MoreThanOneThousand
    }
}
