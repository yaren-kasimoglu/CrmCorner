using Microsoft.AspNetCore.Http;

namespace CrmCorner.ViewModels
{
    public class FinanceImportExcelVm
    {
        public IFormFile ExcelFile { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }
    }
}