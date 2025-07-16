namespace CrmCorner.Models
{
    public class PipelineTaskHistory
    {
        public int Id { get; set; }
        public int PipelineTaskId { get; set; }
        public string ChangedBy { get; set; } // Kullanıcı adı
        public DateTime ChangedAt { get; set; }
        public string ChangedField { get; set; }  // <-- Bu satırı PipelineTaskHistory.cs dosyasına ekle


        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public PipelineTask PipelineTask { get; set; }
    }

}
