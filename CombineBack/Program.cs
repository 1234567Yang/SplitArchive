using System;
using System.IO;
using System.Security.Cryptography;

namespace CombineBack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CombineBack("C:\\C#sourceCode\\SplitArchive\\SplitArchive\\bin\\Debug\\net8.0\\output");
        }



        #region ui
        private const int totalBlock = 50;
        #endregion

        private static bool CombineBack(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            List<string> files = Directory
                .EnumerateFiles(path, "*.splitarchive")
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase) //从小到大，以防万一
                .ToList();
            if (files.Count == 0) return false;

            string originalName = files[0].Split(".0.splitarchive")[0]; //第0个肯定是 .0.splitarchive
            string tempSuffix = ".temp";

            if (File.Exists(originalName + tempSuffix))
            {
                File.Delete(originalName + tempSuffix); // clear cache
            }
            if (File.Exists(originalName))
            {
                File.Delete(originalName);
            }



            #region ui
            var pos = Console.GetCursorPosition();

            for (int i = 0; i < totalBlock; i++)
            {
                Console.Write("|");
            }

            Console.SetCursorPosition(pos.Left, pos.Top);
            #endregion




            FileStream outpustream = new FileStream(originalName + tempSuffix, FileMode.OpenOrCreate);

            outpustream.Position = 0; // just in case

            double finished = 0;
            foreach (string file in files)
            {
                outpustream.Write(File.ReadAllBytes(file));
                finished++;


                #region ui

                while (finished >= files.Count * 1.0 / totalBlock)
                {
                    finished -= files.Count * 1.0 / totalBlock;
                    Console.Write("█");
                    Thread.Sleep(10);
                }

                #endregion
            }


            Console.WriteLine("");
            Console.WriteLine("");// just to make a new line (the UI will not make a new line)



            #region hash
            // https://stackoverflow.com/questions/18535427/how-to-create-sha256-hash-of-downloaded-text-file
            SHA256 SHA256 = SHA256Managed.Create();

            outpustream.Position = 0;

            byte[] hashValue = SHA256.ComputeHash(outpustream);

            string hashFilePath = Path.Combine(
                        path,
                        $"{originalName}.hash"
                        );


            if (!File.Exists(hashFilePath))
            {
                WriteLineWarning("No hash file was found! Press 1 to continue, or anything else to quit.");
                if (Console.ReadLine() != "1")
                {
                    File.Delete(originalName + tempSuffix);
                }
            }
            else
            {
                if (File.ReadAllText(hashFilePath) != BitConverter.ToString(hashValue))
                {
                    WriteLineWarning("The hash is not the same as recorded! Press 1 to continue, or anything else to quit.");
                    if (Console.ReadLine() != "1")
                    {
                        File.Delete(originalName + tempSuffix);
                    }
                }
                else
                {
                    WriteLineSuccess($"Verified hash {BitConverter.ToString(hashValue)}.");
                }
            }

            #endregion




            outpustream.Flush();
            outpustream.Close();



            File.Move(originalName + tempSuffix, originalName); //https://stackoverflow.com/questions/3218910/rename-a-file-in-c-sharp


            Console.WriteLine($"Finished! \nFile path: {path} \nFile name: {originalName}");



            return true;
        }






        public static void WriteLineSuccess(string text)
        {
            WriteWithColor(text + "\n\r", ConsoleColor.Black, ConsoleColor.Green);
        }
        public static void WriteLineWarning(string text)
        {
            WriteWithColor(text + "\n\r", ConsoleColor.Black, ConsoleColor.Yellow);
        }
        public static void WriteWithColor(string text, ConsoleColor bg = ConsoleColor.Black, ConsoleColor fg = ConsoleColor.White)
        {
            lock (Console.Out)
            {
                ConsoleColor ori_bg = Console.BackgroundColor;
                ConsoleColor ori_fg = Console.ForegroundColor;

                Console.BackgroundColor = bg;
                Console.ForegroundColor = fg;
                Console.Write(text);

                Console.BackgroundColor = ori_bg;
                Console.ForegroundColor = ori_fg;
            }
        }
    }
}
