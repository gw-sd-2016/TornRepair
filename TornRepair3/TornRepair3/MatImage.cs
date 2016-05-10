using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace TornRepair3
{
    public class MatImage
    {
        private Mat matImage; // source
        private Mat destImage; // destination
        private Image<Bgr, byte> img;

        // constructor
        public MatImage()
        {
            
        }
        public MatImage(Mat m)
        {
            matImage = m;
            destImage = m.Clone();
            
        }
        // get properties
       
        // manipulations
        // convert to grayscale
        public void Convert()
        {
            CvInvoke.CvtColor(matImage, destImage, ColorConversion.Bgr2Gray);
        }
        public void SmoothGaussian(double sigma)
        {
            CvInvoke.GaussianBlur(matImage, destImage, new Size(), sigma);
        }
        public void ThresholdBinaryInv(double threshold, double maxValue)
        {
            CvInvoke.Threshold(matImage, destImage, threshold, maxValue, ThresholdType.BinaryInv);
        }
        public void MorphologyEx(int iterations)
        {
            CvInvoke.MorphologyEx(matImage, destImage, MorphOp.Close, matImage,new Point(), iterations,BorderType.Default,new MCvScalar());
        }
        public VectorOfVectorOfPoint FindContours()
        {
            VectorOfVectorOfPoint contours =new VectorOfVectorOfPoint();
            IOutputArray hierarchy=new Mat();
            CvInvoke.FindContours(matImage, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);
            return contours;
        }

        // scale, because this will cause aliasing, I will use the old method
        public void ResizeTo(int x, int y)
        {
            CvInvoke.Resize(matImage, destImage, new Size(x,y));
            
        }
        public void Resize(double scale)
        {
            CvInvoke.Resize(matImage, destImage, new Size(), scale, scale);
            
           
        }
        // translation
        public void Translate(double x, double y)
        {
           

            Matrix<double> mat = new Matrix<double>(2,3);
            mat[0, 0] = 1;
            mat[0, 1] = 0;
            mat[0, 2] = x;
            mat[1, 0] = 0;
            mat[1, 1] = 1;
            mat[1, 2] = y;


            CvInvoke.WarpAffine(matImage, destImage, mat,new Size(matImage.Width,matImage.Height));
            
        }
        // rotation, theta is in degree
        public void Rotate(double theta,Bgr color)
        {
            Mat Tmat = new Mat();
            // CvInvoke considers rotation differently to Emgu CV image wrapper, theta is actually -theta
            CvInvoke.GetRotationMatrix2D(new PointF(matImage.Width / 2, matImage.Height / 2), -theta, 1,Tmat);
            CvInvoke.WarpAffine(matImage, destImage, Tmat, new Size(matImage.Width, matImage.Height),borderValue:new MCvScalar(color.Blue,color.Green,color.Red));
        }
        // output
        public Image<Bgr,byte> Output()
        {
            return destImage.ToImage<Bgr,byte>();
        }
        public Mat Out()
        {
            return destImage;
        }
       
    }
}
