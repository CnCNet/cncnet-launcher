using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CnCNetLauncher
{
    public static class Log
    {
        private static BufferedStream logStream;

        public static void Open()
        {
            if (logStream != null)
                return;

            var pathSplit = Application.ExecutablePath.Split(Path.DirectorySeparatorChar);
            var exe = pathSplit[pathSplit.Length - 1];
            var exeSplit = exe.Split('.');
            var logFile = Configuration.FilePath(exeSplit[0] + ".log");

            Console.WriteLine("Opening log from " + logFile);

            var fs = new FileStream(logFile, FileMode.Append);
            if (fs != null)
                logStream = new BufferedStream(fs);
        }

        public static void Info(string format, params object[] args)
        {
            Write("I", new StackFrame(1, true), format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            Write("W", new StackFrame(1, true), format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Write("E", new StackFrame(1, true), format, args);
        }

        public static void Write(string prefix, StackFrame frame, string format, params object[] args)
        {
            var method = frame.GetMethod();
            var line = String.Format("[{0}] {1}.{2}: {3}\r\n", prefix, method.DeclaringType.Name, method.Name, String.Format(format, args));
            var bytes = Encoding.UTF8.GetBytes(line);

            Console.Write(line);

            if (logStream != null && logStream.CanWrite)
            {
                logStream.Write(bytes, 0, bytes.Length);
                logStream.FlushAsync();
            }
        }

        public static void Close()
        {
            if (logStream != null)
            {
                logStream.Close();
                logStream.Dispose();
                logStream = null;
            }
        }
    }
}

