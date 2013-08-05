
namespace System.Xml.Linq
{
    internal static class LinqToXmlExtensions
    {
        public static string ValueOrDefault(this XAttribute attribute, string defaultValue = "")
        {
            if (attribute != null)
                return attribute.Value;
            else
                return defaultValue;
        }

        public static string ValueOrDefault(this XElement element, string defaultValue = "")
        {
            if (element != null)
                return element.Value;
            else
                return defaultValue;
        }
    }
}
