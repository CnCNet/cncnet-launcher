using System.IO;

namespace CnCNetLauncher
{
    public class Utils
    {
        private Utils ()
        {
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buf = new byte[4096];
            int len; 

            while ((len = input.Read(buf, 0, buf.Length)) > 0) {
                output.Write(buf, 0, len);
            }
        }
    }
}

