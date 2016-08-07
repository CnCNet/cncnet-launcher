using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Packager
{
    class MainClass
    {
        static List<string> buildFileList(string dir, List<string> list = null)
        {
            if (list == null)
                list = new List<string>();

            foreach (var file in Directory.GetFiles(dir))
            {
                list.Add(file);
            }

            foreach (var subDir in Directory.GetDirectories(dir))
            {
                buildFileList(subDir, list);
            }

            return list;
        }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: cncnet-packager.exe <source directory> <destination directory>");
                return;
            }

            string srcDir = args[0];
            string dstDir = args[1];

            if (srcDir.Length == 0 || dstDir.Length == 0)
            {
                Console.WriteLine("Both source and destination directories are required.");
                return;
            }

            if (srcDir[srcDir.Length - 1] != Path.DirectorySeparatorChar
                && srcDir[srcDir.Length - 1] != '/'
                && srcDir[srcDir.Length - 1] != '\\'
               )
            {
                srcDir += Path.DirectorySeparatorChar;
            }

            if (dstDir[dstDir.Length - 1] != Path.DirectorySeparatorChar
                && dstDir[dstDir.Length - 1] != '/'
                && dstDir[dstDir.Length - 1] != '\\'
               )
            {
                dstDir += Path.DirectorySeparatorChar;
            }

            try
            {
                if ((File.GetAttributes(srcDir) & FileAttributes.Directory) == 0)
                {
                    Console.WriteLine("{0} is not a directory.", srcDir);
                    return;
                }
            }
            catch (FileNotFoundException) {
                Console.WriteLine("{0} does not exist.", srcDir);
                return;
            }

            try
            {
                if ((File.GetAttributes(dstDir) & FileAttributes.Directory) == 0)
                {
                    Console.WriteLine("{0} is not a directory.", dstDir);
                }
            }
            catch (FileNotFoundException) {
                Console.WriteLine("{0} does not exist.", dstDir);
                return;
            }

            List<string> rawSrcList = buildFileList(srcDir);
            List<string> srcList = new List<string>();

            foreach (var f in rawSrcList)
            {
                srcList.Add(f.Substring(srcDir.Length));
            }

            srcList.Sort();

            StringBuilder manifest = new StringBuilder();

            foreach (var f in srcList)
            {
                if (f == "manifest.txt")
                {
                    Console.WriteLine("Warning: Skipping manifest.txt from source directory.");
                    continue;
                }

                Console.WriteLine("Hashing {0}...", f);

                string hashString;

                try
                {
                    using (FileStream fs = new FileStream(srcDir + f, FileMode.Open))
                    using (BufferedStream bs = new BufferedStream(fs))
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        byte[] hash = sha1.ComputeHash(bs);
                        StringBuilder formatted = new StringBuilder(2 * hash.Length);
                        foreach (byte b in hash)
                        {
                            formatted.AppendFormat("{0:x2}", b);
                        }
                        hashString = formatted.ToString();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error hashing {0}: " + e.Message, f);
                    return;
                }

                if (!File.Exists(dstDir + hashString + ".gz"))
                {
                    Console.WriteLine("Compressing to {0}.gz...", hashString);

                    using (var ifs = new FileStream(srcDir + f, FileMode.Open))
                    using (var ofs = new FileStream(dstDir + hashString + ".gz", FileMode.Create))
                    using (var gzs = new GZipStream(ofs, CompressionLevel.Optimal))
                    {
                        ifs.CopyTo(gzs);
                    }
                }
                else
                {
                    Console.WriteLine(hashString + ".gz exists, skipping compression.");
                }

                var fi = new FileInfo(dstDir + hashString + ".gz");

                manifest.AppendLine(hashString + " " + fi.Length + " " + f.Replace(Path.DirectorySeparatorChar, '/'));
            }

            File.WriteAllText(dstDir + "manifest.txt", manifest.ToString());

            Console.WriteLine("Manifest written to " + dstDir + "manifest.txt");
        }
    }
}
