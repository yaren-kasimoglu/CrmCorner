using System;

namespace CrmCorner.Helpers
{
    public static class InvoiceDeadlineHelper
    {
        // Kural:
        // invoiceDate + 7 gün, ama eğer ay sonuna 7 günden az kalıyorsa deadline = ay sonu
        public static DateTime? GetOriginalDeadline(DateTime? invoiceDate)
        {
            if (!invoiceDate.HasValue) return null;

            var d = invoiceDate.Value.Date;
            var plus7 = d.AddDays(7);
            var monthEnd = new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));

            return plus7 <= monthEnd ? plus7 : monthEnd;
        }

        // Bugün ile deadline arası gün farkı:
        // 3 => 3 gün kaldı
        // 0 => bugün son gün
        // -2 => 2 gün gecikti
        public static int? GetDaysLeft(DateTime? invoiceDate, DateTime today)
        {
            var deadline = GetOriginalDeadline(invoiceDate);
            if (!deadline.HasValue) return null;

            return (deadline.Value.Date - today.Date).Days;
        }

        // sadece “son 3 gün” ve “geçmiş günler” uyarı üretmek için
        public static bool ShouldWarn(DateTime? invoiceDate, DateTime today, int threshold = 3)
        {
            var daysLeft = GetDaysLeft(invoiceDate, today);
            if (!daysLeft.HasValue) return false;

            return daysLeft.Value <= threshold; // 3,2,1,0,-1,-2...
        }
    }
}
