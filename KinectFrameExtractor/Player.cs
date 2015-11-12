using System;
using System.Threading;
using Microsoft.Kinect.Tools;

namespace KinectFrameExtractor
{
    class Player
    {
        private string fileName { get; set; }

        public Player(string fileName)
        {
            this.fileName = fileName;
        }

        public void PlayXef()
        {
            //https://social.msdn.microsoft.com/Forums/en-US/59c97d1e-79f6-4dd0-8fae-73080a2c7b18/documentation-for-microsoftkinecttools-api?forum=kinectv2sdk
            Console.WriteLine("Creating Kinect Studio client");
            using (KStudioClient client = KStudio.CreateClient())
            {
                Console.WriteLine("Connecting to a service");
                client.ConnectToService();

                using (KStudioPlayback playback = client.CreatePlayback(fileName))
                {
                    Console.WriteLine("Playback created");
                    playback.EndBehavior = KStudioPlaybackEndBehavior.Stop;

                    playback.Start();

                    while (playback.State == KStudioPlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }

                    if (playback.State == KStudioPlaybackState.Error)
                    {
                        throw new InvalidOperationException("Error: Playback failed!");
                    }
                }

                Console.WriteLine("Disconnecting");
                client.DisconnectFromService();
            }
        }
     }
}
