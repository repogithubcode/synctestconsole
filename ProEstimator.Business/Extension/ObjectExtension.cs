using System.IO;
using System.Xml.Serialization;

namespace ProEstimator.Business.Extension
{
    public static class ObjectExtension
    {
        public static string Serialize<T>(this T item)
        {
            string result = string.Empty;
            using (var writer = new StringWriter())
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(T));
                mySerializer.Serialize(writer, item);
                result = writer.ToString();
            }

            return result;
        }
    }
}
