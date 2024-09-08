using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Models.Enums
{
    public enum IndustryType
    {
        [Display(Name = "Otomotiv")]
        Automotive,

        [Display(Name = "İnşaat")]
        Construction,

        [Display(Name = "Danışmanlık")]
        Consulting,

        [Display(Name = "Eğitim")]
        Education,

        [Display(Name = "Enerji")]
        Energy,

        [Display(Name = "Eğlence")]
        Entertainment,

        [Display(Name = "Finans")]
        Finance,

        [Display(Name = "Gıda & İçecek")]
        FoodBeverage,

        [Display(Name = "Sağlık")]
        Healthcare,

        [Display(Name = "Konaklama")]
        Hospitality,

        [Display(Name = "Sigorta")]
        Insurance,

        [Display(Name = "İmalat")]
        Manufacturing,

        [Display(Name = "İlaç")]
        Pharmaceuticals,

        [Display(Name = "Emlak")]
        RealEstate,

        [Display(Name = "Perakende")]
        Retail,

        [Display(Name = "Teknoloji")]
        Technology,

        [Display(Name = "Telekomünikasyon")]
        Telecommunications,

        [Display(Name = "Ulaştırma")]
        Transportation,

        [Display(Name = "Hizmetler")]
        Utilities,

        // Yeni eklenen sektörler
        [Display(Name = "Metal")]
        Metal,

        [Display(Name = "Demir Çelik")]
        IronSteel,

        [Display(Name = "Elektrik & Elektronik")]
        ElectricalElectronics,

        [Display(Name = "Alüminyum")]
        Aluminum,

        [Display(Name = "Üretim")]
        Production,

        [Display(Name = "Mekanik Endüstri")]
        MechanicalIndustry,

        [Display(Name = "Tekstil")]
        Textile,

        [Display(Name = "Lojistik")]
        Logistics,

        [Display(Name = "Makine")]
        Machinery,

        [Display(Name = "Diğer")]
        Other
    }
}
