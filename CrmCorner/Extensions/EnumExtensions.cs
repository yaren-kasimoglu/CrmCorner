using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            // Enum değerinin türünü ve özelliklerini al
            var info = enumValue.GetType().GetField(enumValue.ToString());

            // Display attribute'unun varlığını kontrol et
            var attributes = (DisplayAttribute[])info.GetCustomAttributes(typeof(DisplayAttribute), false);

            // Eğer Display attribute'u varsa, onun Name özelliğini dön
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Name;

            // Eğer Display attribute'u yoksa, enum değerinin adını dön
            return enumValue.ToString();
        }
    }
}
