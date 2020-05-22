using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Credit_Card_OCR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //The video capture stream that holds the video from the camera
        public VideoCapture stream = new VideoCapture(0);

        bool isStream = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //Declare that a video stream is being used
            isStream = true;

            //Start grabbing frames from the webcame input
            stream.ImageGrabbed += Capture_ImageGrabbed1;
            stream.Start();
        }

        private void Capture_ImageGrabbed1(object sender, EventArgs e)
        {
            //Get the frame
            Bitmap frame = CameraConfig.GetFrame(stream);

            //Pass each frame from the video capture to the output image
            this.Dispatcher.Invoke(() =>
            {
                webcamOutput.Source = ImageSourceFromBitmap(frame);
            });
        }

        private void btnOCR_Click(object sender, RoutedEventArgs e)
        {
            //Mat object to hold image that is taken
            Mat img = new Mat();

            //If a video stream is currently happening, do the following, else do the last
            if (isStream == true)
            {
                //Get the taken picture from the video stream
                Image<Bgr, byte> inputImg = CameraConfig.TakePicture(stream);

                //Align the video capture image
                img = AutoAlignment.Align(inputImg.Mat);

                //Pass the bitmap image to be displayed
                imgOutput.Source = ImageSourceFromBitmap(img.ToBitmap());
            }
            else
            {
                //Read in the desired image
                img = OCR.ReadInImage();

                //Convert the image to a bitmap, then to an image source
                Bitmap bit = img.ToBitmap();
                imgOutput.Source = ImageSourceFromBitmap(bit);

            }

            //Detect the text from the image
            string detectedText = OCR.RecognizeText(img);

            //Create a new list for the output
            List<string> output = new List<string>();

            //Add the detected string to the list
            output.Add(detectedText);

            //Display the list
            lstOutput.ItemsSource = output;
        }

        //Convert a bitmap to imagesource
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}
