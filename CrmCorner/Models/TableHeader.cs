using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrmCorner.Models
{
    public class TableHeader
    {
        public int Id { get; set; }
        public string ColumnKey { get; set; }
        public string ColumnName { get; set; }

        public int? CompanyId { get; set; }


        public virtual Company? Company { get; set; }
    }

}
