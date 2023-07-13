using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

//Developer: SangonomiyaSakunovi

namespace SangoKCPNet
{
    public static class KCPTool
    {
        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream outMs = new MemoryStream())
            {
                using (GZipStream gzs = new GZipStream(outMs, CompressionMode.Compress, true))
                {
                    gzs.Write(input, 0, input.Length);
                    gzs.Close();
                    return outMs.ToArray();
                }
            }
        }

        public static byte[] DeCompress(byte[] input)
        {
            using (MemoryStream inputMs = new MemoryStream(input))
            {
                using (MemoryStream outMs = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(inputMs, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int length = 0;
                        while ((length = gzs.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            outMs.Write(bytes, 0, length);
                        }
                        gzs.Close();
                        return outMs.ToArray();
                    }
                }
            }
        }

        public static byte[] Serialize<T>(T messsage) where T : KCPMessage
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, messsage);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
                catch (SerializationException ex)
                {
                    KCPLog.Error("Faild to Serilize.Reason:{0}", ex.Message);
                    throw;
                }
            }
        }

        public static T DeSerialize<T>(byte[] bytes) where T : KCPMessage
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    T message = (T)bf.Deserialize(ms);
                    return message;
                }
                catch (SerializationException ex)
                {
                    KCPLog.Error("Faild to DeSerilize.Reason:{0}, bytesLength:{1}", ex.Message, bytes.Length);
                    throw;
                }
            }
        }
    }
}
