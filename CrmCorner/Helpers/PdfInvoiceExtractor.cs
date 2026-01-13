using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace CrmCorner.Helpers
{
    public static class PdfInvoiceExtractor
    {
        public static (string InvoiceNo, DateTime? InvoiceDate, string RawText) Extract(string pdfPath)
        {
            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
                return (null, null, "");

            // 1) PDF text oku
            var allText = "";
            using (var document = PdfDocument.Open(pdfPath))
            {
                foreach (var page in document.GetPages())
                {
                    var t = page?.Text;
                    if (!string.IsNullOrWhiteSpace(t))
                        allText += "\n" + t;
                }
            }

            if (string.IsNullOrWhiteSpace(allText))
                return (null, null, "");

            // 2) Fatura No
            string invoiceNo = null;

            var noMatch = Regex.Match(
                allText,
                @"Fatura\s*No\s*[:：]?\s*([A-Za-z0-9\/\-_]+)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (noMatch.Success)
            {
                invoiceNo = (noMatch.Groups[1].Value ?? "").Trim();

                // Sonuna yapışan kelimeleri temizle (Fatura / e-Fatura)
                invoiceNo = Regex.Replace(
                    invoiceNo,
                    @"\s*(e-?fatura|fatura)\s*$",
                    "",
                    RegexOptions.IgnoreCase);

                // Boşlukları kaldır
                invoiceNo = invoiceNo.Replace(" ", "");

                // Sadece izinli karakterler kalsın
                invoiceNo = Regex.Replace(invoiceNo, @"[^A-Za-z0-9\/\-_]", "");
            }

            // 3) Fatura Tarihi
            DateTime? invoiceDate = null;

            var dateMatch = Regex.Match(
                allText,
                @"Fatura\s*Tarihi\s*[:：]?\s*(\d{2}[-\.\/]\d{2}[-\.\/]\d{4})",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (dateMatch.Success)
            {
                var raw = dateMatch.Groups[1].Value.Trim();
                var formats = new[] { "dd-MM-yyyy", "dd.MM.yyyy", "dd/MM/yyyy" };

                if (DateTime.TryParseExact(
                    raw,
                    formats,
                    CultureInfo.GetCultureInfo("tr-TR"),
                    DateTimeStyles.None,
                    out var dt))
                {
                    invoiceDate = dt;
                }
            }

            return (invoiceNo, invoiceDate, allText);
        }
    }
}
