using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models.Enums
{

    public enum SourceChannelType
    {
        [Display(Name = "Apollo")]
        Apollo,

        [Display(Name = "LinkedIn")]
        LinkedIn,

        [Display(Name = "Soğuk Arama")]
        ColdCall,

        [Display(Name = "E-posta")]
        Email,

        [Display(Name = "Tavsiye")]
        Referral,

        [Display(Name = "Web Formu")]
        WebsiteForm,

        [Display(Name = "Diğer")]
        Other
    }

}
