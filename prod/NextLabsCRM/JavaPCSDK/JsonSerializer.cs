using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NextLabs.JavaPC.RestAPISDK
{
    public static class JsonSerializer
    {
        public static string SaveToJson(object struceJson)
        {
            string result = string.Empty;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(struceJson.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, struceJson);
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            result = Encoding.ASCII.GetString(dataBytes);
            return result;
        }
        public static T LoadFromJson<T>(string strJson)
        {
            T read = default(T);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream mStream = new MemoryStream(Encoding.ASCII.GetBytes(strJson));
            read = (T)serializer.ReadObject(mStream);
            return read;
        }
    }
}
