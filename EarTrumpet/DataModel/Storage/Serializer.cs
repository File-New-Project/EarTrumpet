using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.DataModel.Storage
{
    public class Serializer
    {
        public static T FromString<T>(string data)
        {
            using (var reader = new StringReader(data))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
        }

        public static string ToString<T>(string key, T value)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
        }
    }
}
