using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Credit_Card_OCR
{
    /// <summary>
    /// Align an image of a credit card for better OCR detection
    /// </summary>
    class AutoAlignment
    { 
        /// <summary>
        /// Get the image from a path
        /// </summary>
        /// <param name="path">The path to the image</param>
        /// <returns>A mat objefct</returns>
        public static Mat GetImage(string path)
        {
            Mat img = CvInvoke.Imread(path, ImreadModes.AnyColor);

            return img;
        }

        /// <summary>
        /// Align an image taken by the user for better OCR readability
        /// </summary>
        /// <param name="inputImage">The image taken with a camera</param>
        /// <returns>An aligned image</returns>
        public static Mat Align(Mat inputImage)
        {
            //The path to the templete image
            //CURENTLY DONT HAVE ONE, SO THIS PATH IS EMPTY
            string templatePath = "";

            //Get the template image and store it as a mat object
            Mat templ = GetImage(templatePath);

            //Create two mat objects to hold output of templ matching
            Mat dest = new Mat();
            Mat mask = new Mat();

            //Find matches
            CvInvoke.MatchTemplate(inputImage, templ, dest, 
                TemplateMatchingType.Ccoeff, mask);

            //Return the found templete
            return dest;
        }
    }
}