using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models.Enums
{
    public enum PipelineStage
    {
        [Display(Name = "Değerlendirilen")]
        Degerlendirilen = 1,

        [Display(Name = "İletişim Kuruldu")]
        IletisimKuruldu = 2,

        [Display(Name = "Toplantı Düzenlendi")]
        ToplantiDuzenlendi = 3,

        [Display(Name = "Teklif Sunuldu")]
        TeklifSunuldu = 4,

        [Display(Name = "Sonuç")]
        Sonuc = 5
    }


}
