using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;

namespace SplitArchive
{
    internal class Program
    {
        static void Main(string[] args)
        {
        FLAG_INDICATE_FILE:
            Console.WriteLine("Please indicate target file: ");
            string file = Console.ReadLine();
            if (!File.Exists(file))
            {
                Console.WriteLine("Invalid file!");
                goto FLAG_INDICATE_FILE;
            }


        FLAG_INDICATE_SIZE:
            int mb;
            try
            {
                Console.WriteLine("Please indicate the size per file in MB ( 1MB <= file size <= 500MB ): ");
                mb = int.Parse(Console.ReadLine());
                if(mb < 1 || mb > 500)
                {
                    throw new Exception();
                }
            }
            catch
            {
                Console.WriteLine("Invalid size!");
                goto FLAG_INDICATE_SIZE;

            }


            if (!SplitWrite(file, "output", MbToByte(mb)))
            {
                Console.WriteLine("Error while trying to spliting the file!");
            }
        }




        #region ui
        private const int totalBlock = 50;
        #endregion
        // size in byte
        private static bool SplitWrite(string targetFile, string outputDir, int perFileSize)
        {
            if (perFileSize <= 10) return false;
            perFileSize -= 10; //实际要小一个byte，保险起见就再小10个

            if (!File.Exists(targetFile)) return false;
            Directory.CreateDirectory(outputDir);


            FileStream sourceStream = new FileStream(targetFile, FileMode.Open, FileAccess.Read);


            #region ui
            var pos = Console.GetCursorPosition();

            for (int i = 0; i < totalBlock; i++)
            {
                Console.Write("|");
            }

            Console.SetCursorPosition(pos.Left, pos.Top);
            #endregion



            try
            {

                int fileIndex = 0;
                byte[] buffer = new byte[perFileSize];
                int bytesRead;

                #region ui
                long totalRead_forUI = 0;
                long addProgress = (long)Math.Ceiling(new System.IO.FileInfo(targetFile).Length * 1.0 / totalBlock);
                #endregion

                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string partFilePath = Path.Combine(
                        outputDir,
                        $"{Path.GetFileName(targetFile)}.{fileIndex}.splitarchive"
                        );


                    #region ui
                    totalRead_forUI += bytesRead;
                    while (totalRead_forUI >= addProgress)
                    {
                        totalRead_forUI -= addProgress;
                        Console.Write("█");
                        Thread.Sleep(10);
                    }
                    #endregion

                    FileStream partStream = new FileStream(partFilePath, FileMode.Create, FileAccess.Write);

                    partStream.Write(buffer, 0, bytesRead);

                    partStream.Flush();
                    partStream.Close();

                    fileIndex++;
                }
            }
            catch (Exception ex)
            {
                sourceStream.Dispose();
                return false;
            }


            Console.WriteLine(""); // just to make a new line (the UI will not make a new line)


            #region hash
            // https://stackoverflow.com/questions/18535427/how-to-create-sha256-hash-of-downloaded-text-file
            SHA256 SHA256 = SHA256Managed.Create();

            sourceStream.Position = 0;

            byte[] hashValue = SHA256.ComputeHash(sourceStream);

            string hashFilePath = Path.Combine(
                        outputDir,
                        $"{Path.GetFileName(targetFile)}.hash"
                        );


            File.WriteAllText(hashFilePath, BitConverter.ToString(hashValue));

            #endregion

            sourceStream.Dispose();
            return true;
        }
        private static int MbToByte(int mb)
        {
            try
            {
                checked
                {
                    return mb * 1024 * 1024;
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}
