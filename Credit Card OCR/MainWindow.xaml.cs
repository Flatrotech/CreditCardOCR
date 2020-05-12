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
        public VideoCapture stream = new VideoCapture(0);

        bool isStream = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            isStream = true;

            stream.ImageGrabbed += Capture_ImageGrabbed1;
            stream.Start();
        }

        private void Capture_ImageGrabbed1(object sender, EventArgs e)
        {
            Mat m = new Mat();
            stream.Retrieve(m);

            Bitmap frame = m.ToBitmap();

            this.Dispatcher.Invoke(() =>
            {
                webcamOutput.Source = ImageSourceFromBitmap(frame);
            });
        }

        private void btnOCR_Click(object sender, RoutedEventArgs e)
        {
            Mat img = new Mat();

            if (isStream == true)
            {
                stream.Pause();

                var randomTest = stream.QueryFrame().ToImage<Bgr, byte>();
                var capture = randomTest.ToBitmap();

                stream.Start();

                Image<Bgr, byte> inputImg = capture.ToImage<Bgr, byte>();

                img = AutoAlignment.Align(inputImg.Mat);

                imgOutput.Source = ImageSourceFromBitmap(img.ToBitmap());
            }
            else
            {
                //Read in the desired image
                img = OCR.ReadInImage();

                //Convert the image to a bitmap, then to an image scource
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