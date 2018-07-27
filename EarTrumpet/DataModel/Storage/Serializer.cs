using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.DataModel.Storage
{
    public class Serializer
    {
        public static T FromString<T>(string data)
        {
            using (TextReader reader = new StringReader(data))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }
        }

        public static string ToString<T>(string key, T value)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                return stringWriter.ToString();
            }
        }
    }
}
