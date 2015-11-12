using System;
using System.IO;
using System.Threading;

namespace KinectFrameExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("'extractor.exe output_folder input.xef' to extract a .xef file.");                
                return;
            }

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string subFolderPath = System.IO.Path.Combine(myPhotos, args[0]);
            Directory.CreateDirectory(subFolderPath);
            
            using (Extractor extractor = new Extractor(subFolderPath))
            {
                extractor.ListenForFrames();

                string xefFileName = args[1];
                Console.WriteLine("Extracting frames from .xef file: {0}", xefFileName);

                Player player = new Player(xefFileName);
                Thread playerThread;
                playerThread = new System.Threading.Thread(new ThreadStart(player.PlayXef));
                playerThread.Start();
                playerThread.Join();

                extractor.Stop();
            }
        }
    }
}
