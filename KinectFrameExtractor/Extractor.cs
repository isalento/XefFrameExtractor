using System;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace KinectFrameExtractor
{
    class Extractor :  IDisposable
    {
        KinectSensor sensor = null;
        DepthFrameReader depthFrameReader = null;
        FrameDescription depthFrameDescription = null;
        private ushort[] depthPixels = null;
        int frameCounter = 0;

        string outputFolder = "";

        public Extractor(string destinationfolder)
        {
            this.outputFolder = destinationfolder;
        }

        public void ListenForFrames()
        {
            sensor = KinectSensor.GetDefault();

            depthFrameReader = sensor.DepthFrameSource.OpenReader();

            depthFrameReader.FrameArrived += Reader_FrameArrived;

            depthFrameDescription = sensor.DepthFrameSource.FrameDescription;

            // allocate space to put the pixels being received and converted
            depthPixels = new ushort[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            sensor.Open();

            Console.WriteLine("Extractor ready to grab frames.");
        }

        public void Stop()
        {
            Console.WriteLine("Stopping");

            if (sensor != null && sensor.IsOpen)
                sensor.Close();
        }

        public void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            frameCounter++;
            Console.Write(".");

            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)))
                        {
                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                WriteableBitmap depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray16, null);

                depthBitmap.WritePixels(
                    new Int32Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight),
                    this.depthPixels,
                    depthBitmap.PixelWidth * (int)this.depthFrameDescription.BytesPerPixel,
                    0);

                // Create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // Create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(depthBitmap));

                string filePath = Path.Combine(this.outputFolder, "Depth-" + frameCounter.ToString().PadLeft(10, '0') + ".png");

                // Try to write the new file to disk
                try
                {
                    // using as FileStream is IDisposable
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Unable to save image to disk.");
                }
            }
        }

        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            //2 bytes per pixel -> 16 bits
            int upperBound = (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel);
            
            // convert depth to a visual representation
            for (int i = 0; i < upperBound; ++i)
            {
                this.depthPixels[i] = frameData[i];
            }
        }

        public void Dispose()
        {
            if(this.depthFrameReader != null)
                this.depthFrameReader.Dispose();
        }
    }
}
