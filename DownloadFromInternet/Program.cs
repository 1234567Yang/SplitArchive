using System;
using System.IO;
using System.Net;

namespace DownloadFromInternet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("output"))
            {
                Directory.CreateDirectory("output");
            }



        FLAG_URI:
            Console.WriteLine("Please indicate your website link with the original file name, for example: https://example.com/path_to_file/file.txt:");

            Uri uri = null;
            try
            {
                uri = new Uri(Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid uri: {ex.Message}");
                goto FLAG_URI;
            }


            string fileName = Path.GetFileName(uri.LocalPath);
            Console.WriteLine($"Filename: {fileName}");



            HttpClient client = new HttpClient();
            int number = 0;
            while (true)
            {
                UriBuilder builder = new UriBuilder(uri);

                builder.Path += $".{number}.splitarchive";

                Uri modifiedUri = builder.Uri;




                string thisFileLink = modifiedUri.AbsoluteUri;
                Console.WriteLine($"Getting #{number} : {thisFileLink}");

                try
                {
                    HttpResponseMessage message = client.GetAsync(thisFileLink).Result;
                    if (message.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Received status code of {(int)message.StatusCode}, break.");
                        break;
                    }

                    string thisOutputFile = $"output\\{fileName}.{number}.splitarchive";
                    //Console.WriteLine(thisOutputFile);
                    if (File.Exists(thisOutputFile))
                    {
                        File.Delete(thisOutputFile);
                    }

                    byte[] buffer = message.Content.ReadAsByteArrayAsync().Result;
                    if (buffer.Length > 0)
                    {
                        File.WriteAllBytes(thisOutputFile, buffer);
                    }
                    else
                    {
                        WriteLineWarning("Get file buffer = 0, break.");
                        break;
                    }


                }
                catch (Exception ex)
                {
                    WriteLineWarning($"Met an exception: {ex.Message}\n{ex.StackTrace}\n break.");
                    break;
                }


                number++;
            }




            #region get hash
            {
                UriBuilder builder = new UriBuilder(uri);

                builder.Path += ".hash";

                Uri modifiedUri = builder.Uri;

                HttpResponseMessage message = client.GetAsync(modifiedUri.AbsoluteUri).Result;





                string thisOutputFile = $"output\\{fileName}.hash";
                //Console.WriteLine(thisOutputFile);
                if (File.Exists(thisOutputFile))
                {
                    File.Delete(thisOutputFile);
                }

                string hashStr = message.Content.ReadAsStringAsync().Result;
                if (!String.IsNullOrEmpty(hashStr))
                {
                    if (hashStr.Length != 95)
                    {
                        WriteLineWarning($"Warning: possibly invalid hash length: {hashStr.Length}");
                    }
                    File.WriteAllText(thisOutputFile, hashStr);
                    WriteLineSuccess($"Successfully get hash: {(
                        (hashStr.Length > 95) ?
                        hashStr.Substring(0, 95) :
                        hashStr
                        )}");
                }
                else
                {
                    WriteLineWarning($"Get hash len = 0");
                }
            }

            #endregion
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
