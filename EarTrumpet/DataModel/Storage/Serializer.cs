using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace EarTrumpet.DataModel.Storage
{
    public class Serializer
    {
        public static T FromString<T>(string data)
        {
            using var reader = new StringReader(data);
            using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            return (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
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
