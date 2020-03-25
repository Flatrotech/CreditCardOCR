﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace Credit_Card_OCR
{
    /// <summary>
    /// Detect text on an image
    /// </summary>
    class OCR
    {
        //Proccess flow of algorithm:
        //Read in the image
        //Pass it through a veriety of filters
        //Find contours
        //Sort contours from left to right
        //Read all text in each ROI


        //Declare a new Tesseract OCR engine
        private static Tesseract _ocr;

        /// <summary>
        /// Set the dictionary and whitelist for Tesseract
        /// </summary>
        /// <param name="dataPath"></param>
        public static void SetTesseractObjects(string dataPath)
        {
            //create OCR engine
            _ocr = new Tesseract(dataPath, "eng", OcrEngineMode.TesseractLstmCombined);
            _ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ-1234567890/");
        }

        /// <summary>
        /// Read in the image to be used for OCR
        /// </summary>
        /// <returns>A Mat object</returns>
        public static Mat ReadInImage()
        {
            //Change this file path to the path where the images you want to stich are located
            string filePath = Directory.GetParent(Directory.GetParent
                (Environment.CurrentDirectory).ToString()).ToString() + @"/Images/creditCard2.png";

            //Read in the image from the filepath
            Mat img = CvInvoke.Imread(filePath, ImreadModes.AnyColor);

            //Return the image
            return img;
        }

        /// <summary>
        /// Pass the image through multiple filters and sort contours
        /// </summary>
        /// <param name="img">The image that will be proccessed</param>
        /// <returns>A list of Mat ROIs</returns>
        private static List<Mat> ImageProccessing(Mat img)
        {
            //This variable is used for debugging purposes
            Mat smallerOutput = new Mat();

            //Resize the image for better uniformitty throughout the code
            CvInvoke.Resize(img, img, new Size(700, 500));

            Mat imgClone = img.Clone();

            //Convert the image to grayscale
            CvInvoke.CvtColor(img, img, ColorConversion.Bgr2Gray);

            //Blur the image
            CvInvoke.GaussianBlur(img, img, new Size(5, 5), 8, 8);

            //Output the denoised canny image
            CvInvoke.Resize(img, smallerOutput, new Size(img.Width, img.Height));
            CvInvoke.Imshow("Blurred Image", smallerOutput);

            CvInvoke.AdaptiveThreshold(img, img, 30, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 5, 6);

            //Output the denoised canny image
            CvInvoke.Resize(img, smallerOutput, new Size(img.Width, img.Height));
            CvInvoke.Imshow("Thresholded Image", smallerOutput);

            //Canny the image
            CvInvoke.Canny(img, img, 8, 8);

            //Output the denoised canny image
            CvInvoke.Resize(img, smallerOutput, new Size(img.Width, img.Height));
            CvInvoke.Imshow("Canny Image", smallerOutput);

            //Dilate the canny image
            CvInvoke.Dilate(img, img, null, new Point(-1, -1), 8, BorderType.Constant, new MCvScalar(0, 255, 255));

            List<Mat> foundOutput = FindandFilterContours(imgClone, img);

            return foundOutput;
        }

        /// <summary>
        /// Find and sort contours found on the filtered image
        /// </summary>
        /// <param name="originalImage">The original unaltered image</param>
        /// <param name="filteredImage">The filtered image</param>
        /// <returns>A list of ROI mat objects</returns>
        private static List<Mat> FindandFilterContours(Mat originalImage, Mat filteredImage)
        {
            Mat smallerOutput = new Mat();

            //Create a blank image that will be used to display contours
            Image<Bgr, byte> blankImage = new Image<Bgr, byte>(originalImage.Width, originalImage.Height);
            //Clone the input image
            Image<Bgr, byte> originalImageClone = originalImage.Clone().ToImage<Bgr, byte>();

            //Declare a new vector that will store contours
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            //Find and draw the contours on the blank image
            CvInvoke.FindContours(filteredImage, contours, null, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(blankImage, contours, -1, new MCvScalar(255, 0, 0));

            //Show the blank image with contours drawn on it
            CvInvoke.Resize(blankImage, smallerOutput, new Size(originalImage.Width, originalImage.Height));
            CvInvoke.Imshow("Contour Image", smallerOutput);

            //Create two copys of the cloned image of the input image
            Image<Bgr, byte> allContoursDrawn = originalImageClone.Copy();
            Image<Bgr, byte> finalCopy = originalImageClone.Copy();

            //Create two lists that will be used elsewhere in the algorithm
            List<Rectangle> listRectangles = new List<Rectangle>();
            List<int> listXValues = new List<int>();

            //Loop over all contours
            for (int i = 0; i < contours.Size; i++)
            {
                //Create a bounding rectangle around each contour
                Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                originalImageClone.ROI = rect;

                //Add the bounding rectangle and its x value to their corresponding lists
                listRectangles.Add(rect);
                listXValues.Add(rect.X);

                //Draw the bounding rectangle on the image
                allContoursDrawn.Draw(rect, new Bgr(255, 0, 0), 5);
            }

            //Create two new lists that will hold data in the algorithms later on
            List<int> indexList = new List<int>();
            List<int> smallerXValues = new List<int>();

            //Loop over all relevent information
            for (int i = 0; i < listRectangles.Count; i++)
            {
                //If a bounding rectangle fits certain dementions, add it's x value to another list
                if ((listRectangles[i].Width < 400) && (listRectangles[i].Height < 400)
                    && (listRectangles[i].Y > 200) && (listRectangles[i].Y < 300) && 
                    (listRectangles[i].Width > 50) && (listRectangles[i].Height > 50))
                {
                    originalImageClone.ROI = listRectangles[i];

                    finalCopy.Draw(listRectangles[i], new Bgr(255, 0, 0), 5);

                    smallerXValues.Add(listRectangles[i].X);
                }
            }

            //Sort the smaller list into asending order
            smallerXValues.Sort();

            //Loop over each value in the sorted list, and check if the same value is in the original list
            //If it is, add the index of the that value in the original list to a new list
            for (int i = 0; i < smallerXValues.Count; i++)
            {
                for (int j = 0; j < listXValues.Count; j++)
                {
                    if (smallerXValues[i] == listXValues[j])
                    {
                        indexList.Add(j);
                    }
                }
            }

            //A list to hold the final ROIs
            List<Mat> outputImages = new List<Mat>();

            //Loop over the sorted indexes, and add them to the final list
            for (int i = 0; i < indexList.Count; i++)
            {
                originalImageClone.ROI = listRectangles[indexList[i]];

                outputImages.Add(originalImageClone.Clone().Mat);
            }

            CvInvoke.Resize(allContoursDrawn, smallerOutput, new Size(originalImage.Width, originalImage.Height));
            CvInvoke.Imshow("Boxes Drawn on Image", smallerOutput);

            CvInvoke.Resize(finalCopy, smallerOutput, new Size(originalImage.Width, originalImage.Height));
            CvInvoke.Imshow("Boxes Drawn on FinalCopy", smallerOutput);

            return outputImages;
        }

        /// <summary>
        /// Detects text on an image
        /// </summary>
        /// <param name="img">The image where text will be extracted from</param>
        /// <returns>A string of detected text</returns>
        public static string RecognizeText(Mat img)
        {
            //Change this file path to the path where the images you want to stich are located
            string filePath = Directory.GetParent(Directory.GetParent
                (Environment.CurrentDirectory).ToString()).ToString() + @"/Tessdata/";

            //Declare the use of the dictonary
            SetTesseractObjects(filePath);

            //Get all cropped regions
            List<Mat> croppedRegions = ImageProccessing(img);

            string output = "";

            Tesseract.Character[] words;

            //Loop over all ROIs and detect text on each image
            for (int i = 0; i < croppedRegions.Count; i++)
            {
                StringBuilder strBuilder = new StringBuilder();

                //Set and detect text on the image
                _ocr.SetImage(croppedRegions[i]);
                _ocr.Recognize();

                words = _ocr.GetCharacters();

                for (int j = 0; j < words.Length; j++)
                {
                    strBuilder.Append(words[j].Text);
                }

                //Pass the stringbuilder into a string variable
                output += strBuilder.ToString() + " ";
            }

            //Return a string
            return output;
        }
    }
}