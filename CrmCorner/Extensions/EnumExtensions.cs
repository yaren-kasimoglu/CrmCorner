using System.ComponentModel.DataAnnotations;

namespace CrmCorner.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty; ;
            }

            var info = enumValue.GetType().GetField(enumValue.ToString());

            var attributes = (DisplayAttribute[])info.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Name;
            }

            return enumValue.ToString();
        }
    }
}
