using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models.Enums
{
    public enum OutcomeTypeSales
    {
        [Display(Name = "Hiçbiri")]
        None,
        [Display(Name = "Kazanıldı")]
        Won,
        [Display(Name = "Kaybedildi")]
        Lost,
    }
}
