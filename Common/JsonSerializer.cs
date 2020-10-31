using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Common
{
    public static class JsonSerializer<T>
    {
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;

            var serializer = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                return stream.ToArray();
            }
        }

        public static T Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
