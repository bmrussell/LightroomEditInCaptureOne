using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace source
{
    class Program
    {
        static void Main(string[] args)
        {
            // Expect one agrument of the form:
            // F:\Lightroom\Images\2018\2018-10-28\_DSF0116.C1.tif
            // {folder}\{basename}.C1.{ext}
            var cfg = InitOptions<AppConfig>();

            if (args.Length == 0)
            {
                Console.WriteLine("No image parameter supplied");
                Console.ReadLine();
            }
            else
            {
                var tiffFile = args[0];
                var ext = Path.GetExtension(tiffFile).ToUpper();
                if (ext != ".TIF" && ext != ".PSD")
                {
                    Console.WriteLine("Only works with .TIF or PSD to guard against deleting Lightroom raw files");
                    System.Environment.Exit(1);
                }
                Console.WriteLine("Looking for raw file for {0}", tiffFile);
                var rawPath = Path.GetDirectoryName(tiffFile);
                var rawFile = Path.GetFileNameWithoutExtension(tiffFile);
                if (rawFile.Substring(rawFile.Length - 3).ToUpper() == ".C1")
                {
                    rawFile = rawFile.Substring(0, rawFile.Length - 3);
                    foreach (var rawType in cfg.RawFileTypes)
                    {
                        var candidate = Path.Join(rawPath, rawFile + "." + rawType);
                        if (File.Exists(candidate))
                        {
                            Console.WriteLine("Found {0}.", candidate);
                            Console.WriteLine("Starting Capture One and removing Lightroom TIFF...");
                            File.Delete(tiffFile);
                            StartProgram(cfg.CaptureOnePath, candidate);
                        }
                    }
                }

            }

            static void StartProgram(string program, string parameter)
            {
                using (var myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = false;
                    // You can start any process, HelloWorld is a do-nothing example.
                    myProcess.StartInfo.FileName = program;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.Arguments = parameter;
                    myProcess.Start();
                }
            }
        }


        private static T InitOptions<T>()
            where T : new()
        {
            var config = InitConfig();
            return config.Get<T>();
        }

        private static IConfigurationRoot InitConfig()
        {            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }



}


