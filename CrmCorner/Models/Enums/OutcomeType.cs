using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models.Enums
{
    public enum OutcomeType
    {
        Olumlu,
        Olumsuz,
        [Display(Name = "Süreçte")]
        Surecte
    }
}
