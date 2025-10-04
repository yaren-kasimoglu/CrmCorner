namespace CrmCorner.ViewModels
{
    public class StageByUserRowVm
    {
        public string ResponsibleUserName { get; set; }
        public int Degerlendirilen { get; set; }
        public int IletisimKuruldu { get; set; }
        public int ToplantiDuzenlendi { get; set; }
        public int TeklifSunuldu { get; set; }
        public int Sonuc { get; set; }

        public Dictionary<string, List<string>> TaskTitlesByStage { get; set; } = new();
    }
}
