using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Credit_Card_OCR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOCR_Click(object sender, RoutedEventArgs e)
        {
            //Read in the desired image
            Mat img = OCR.ReadInImage();

            //Convert the image to a bitmap, then to an image scource
            Bitmap bit = img.ToBitmap();
            imgOutput.Source = ImageSourceFromBitmap(bit);

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
