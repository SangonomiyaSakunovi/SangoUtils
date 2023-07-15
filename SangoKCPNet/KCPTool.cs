using System;
using System.IO;
using System.IO.Compression;

//Developer: SangonomiyaSakunovi

namespace SangoKCPNet
{
    public static class KCPTool
    {
        public static readonly DateTimeOffset UTCStartTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        public static ulong GetUTCStartMillseconds()
        {
            TimeSpan timeSpan = DateTimeOffset.UtcNow - UTCStartTime;
            return (ulong)timeSpan.TotalMilliseconds;
        }

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
    }
}
