using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using vJoyWrapper;

namespace Gugalnik
{
    //  public class Grafika
    public partial class Form1 : Form

    {
        vJoyFunctions vw ;
        double xtrans =0;
        double ytrans =0;

        Rectangle leftSideSlider = new Rectangle(9, 99, 80, 24);
        Rectangle rightSideSlider = new Rectangle(135, 99, 80, 24);
        Rectangle frontSideSlider = new Rectangle(100, 10, 24, 80);
        Rectangle backSideSlider = new Rectangle(100, 134, 24, 80);
        Rectangle upDownSideSlider = new Rectangle(295, 67, 33, 90);
        Rectangle driftSideSlider = new Rectangle(49, 225, 126, 24);

        int leftSideLine = 47;
        int rightSideLine = 182;
        int frontSideLine = 50;
        int backSideLine = 173;
        int upDownSideLine = 112;
        int driftSideLine = 113;

        Point FLpoint = new Point(89, 50);
        Point FLpointR = new Point(49, 50);

        Point FRpoint = new Point(135, 50);
        Point FRpointR = new Point(175, 50);

        Point RLpoint = new Point(89, 174);
        Point RLpointR = new Point(49, 174);

        Point RRpoint = new Point(135, 174);
        Point RRpointR = new Point(175, 174);

        int radiousMotorPosCircle = 40;

        Point circle = new Point(255, 112);
        Point circlePoint = new Point(255, 112);
        int radiousCircle = 29;

        public void pictureBox1_Click(object sender, EventArgs e)
        {
            var mouseEvent = (MouseEventArgs)e;

            Point clickPoint = new Point(mouseEvent.X, mouseEvent.Y);
            //if (IsClickInsideRectangle(clickPoint, leftSideSlider) == true)
            //{
            //    leftSideLine = clickPoint.X;
            //    pfl = TransformValueWidth(leftSideLine, leftSideSlider);
            //    prl = TransformValueWidth(leftSideLine, leftSideSlider);
            //}
            //if (IsClickInsideRectangle(clickPoint, rightSideSlider) == true)
            //{
            //    rightSideLine = clickPoint.X;
            //    pfr = TransformValueWidth(rightSideLine, rightSideSlider);
            //    prr = TransformValueWidth(rightSideLine, rightSideSlider);

            //}
            //if (IsClickInsideRectangle(clickPoint, frontSideSlider) == true)
            //{
            //    frontSideLine = clickPoint.Y;
            //    pfr = TransformValueHeight(frontSideLine, frontSideSlider);
            //    pfl = TransformValueHeight(frontSideLine, frontSideSlider);
            //}
            //if (IsClickInsideRectangle(clickPoint, backSideSlider) == true)
            //{
            //    backSideLine = clickPoint.Y;
            //    prr = TransformValueHeight(backSideLine, backSideSlider);
            //    prl = TransformValueHeight(backSideLine, backSideSlider);

            //}
            //if (IsClickInsideRectangle(clickPoint, upDownSideSlider) == true)
            //{
            //    upDownSideLine = clickPoint.Y;
            //    prr = TransformValueHeight(upDownSideLine, upDownSideSlider);
            //    prl = TransformValueHeight(upDownSideLine, upDownSideSlider);
            //    pfr = TransformValueHeight(upDownSideLine, upDownSideSlider);
            //    pfl = TransformValueHeight(upDownSideLine, upDownSideSlider);

            //}
            //if (IsClickInsideRectangle(clickPoint, driftSideSlider) == true)
            //{
            //    driftSideLine = clickPoint.X;
            //    prl = TransformValueWidth(driftSideLine, driftSideSlider);
            //    prr = 180 - TransformValueWidth(driftSideLine, driftSideSlider);

            //}

            if (IsClickInsideCircle(clickPoint, circle, radiousCircle) == true)
            {
                circlePoint.X = clickPoint.X;
                circlePoint.Y = clickPoint.Y;
                 xtrans = ((circlePoint.X - circle.X) / (double)radiousCircle);
                 ytrans = ((circlePoint.Y - circle.Y) / (double)radiousCircle);

            }

            //if (IsClickInsideCircle(clickPoint, FLpoint, radiousMotorPosCircle) == true && clickPoint.X < FLpoint.X)
            //{
            //    FLpointR = getCircleCroxx(clickPoint, FLpoint, radiousMotorPosCircle);
            //    pfl = 90 + getCircleCroxxAngle(clickPoint, FLpoint, radiousMotorPosCircle);

            //}
            //if (IsClickInsideCircle(clickPoint, FRpoint, radiousMotorPosCircle) == true && clickPoint.X > FRpoint.X)
            //{
            //    FRpointR = getCircleCroxx(clickPoint, FRpoint, radiousMotorPosCircle);
            //    pfr = 90 - getCircleCroxxAngle(clickPoint, FRpoint, radiousMotorPosCircle);

            //}
            //if (IsClickInsideCircle(clickPoint, RLpoint, radiousMotorPosCircle) == true && clickPoint.X < RLpoint.X)
            //{
            //    RLpointR = getCircleCroxx(clickPoint, RLpoint, radiousMotorPosCircle);
            //    prl = 90 + getCircleCroxxAngle(clickPoint, RLpoint, radiousMotorPosCircle);
            //}
            //if (IsClickInsideCircle(clickPoint, RRpoint, radiousMotorPosCircle) == true && clickPoint.X > RRpoint.X)
            //{
            //    RRpointR = getCircleCroxx(clickPoint, RRpoint, radiousMotorPosCircle);
            //    prr = 90 - getCircleCroxxAngle(clickPoint, RRpoint, radiousMotorPosCircle);
            //}

            //MessageBox.Show(prl.ToString() + " " + prr.ToString());
            ((PictureBox)sender).Invalidate();
        }

        private bool IsClickInsideRectangle(Point clickPoint, Rectangle rectangle)
        {
            if (clickPoint.X >= rectangle.X && clickPoint.X <= rectangle.Right && clickPoint.Y >= rectangle.Y && clickPoint.Y <= rectangle.Bottom) return true;
            else return false;
        }

        private bool IsClickInsideCircle(Point clickPoint, Point centre, int radious)
        {
            if ((Math.Pow(clickPoint.X - centre.X, 2) + Math.Pow(clickPoint.Y - centre.Y, 2)) < (radious * radious))
                return true;
            else return false;
        }

        public void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangles(Brushes.Aquamarine, new Rectangle[] { leftSideSlider, rightSideSlider, frontSideSlider, backSideSlider, upDownSideSlider, driftSideSlider });

            e.Graphics.DrawLine(Pens.Black, leftSideLine, leftSideSlider.Y, leftSideLine, leftSideSlider.Bottom);
            e.Graphics.DrawLine(Pens.Black, rightSideLine, rightSideSlider.Y, rightSideLine, rightSideSlider.Bottom);
            e.Graphics.DrawLine(Pens.Black, frontSideSlider.X, frontSideLine, frontSideSlider.Right, frontSideLine);
            e.Graphics.DrawLine(Pens.Black, backSideSlider.X, backSideLine, backSideSlider.Right, backSideLine);
            e.Graphics.DrawLine(Pens.Black, upDownSideSlider.X, upDownSideLine, upDownSideSlider.Right, upDownSideLine);

            DrawHalfCircle(e, radiousMotorPosCircle, FLpoint, 90f, 180f);
            DrawHalfCircle(e, radiousMotorPosCircle, FRpoint, -90f, 180f);
            DrawHalfCircle(e, radiousMotorPosCircle, RLpoint, 90f, 180f);
            DrawHalfCircle(e, radiousMotorPosCircle, RRpoint, -90f, 180f);
            e.Graphics.FillEllipse(Brushes.Bisque, new Rectangle(circle.X - radiousCircle, circle.Y - radiousCircle, 2 * radiousCircle, 2 * radiousCircle));
            e.Graphics.FillEllipse(Brushes.YellowGreen, new Rectangle(circle.X - 2, circle.Y - 2, 4, 4));
            e.Graphics.DrawLine(Pens.Black, FLpointR.X, FLpointR.Y, FLpoint.X, FLpoint.Y);
            e.Graphics.DrawLine(Pens.Black, FRpointR.X, FRpointR.Y, FRpoint.X, FRpoint.Y);
            e.Graphics.DrawLine(Pens.Black, RLpointR.X, RLpointR.Y, RLpoint.X, RLpoint.Y);
            e.Graphics.DrawLine(Pens.Black, RRpointR.X, RRpointR.Y, RRpoint.X, RRpoint.Y);
            e.Graphics.DrawLine(Pens.Black, driftSideLine, driftSideSlider.Y, driftSideLine, driftSideSlider.Bottom);
            e.Graphics.FillEllipse(new SolidBrush(Color.Red), circlePoint.X - 3, circlePoint.Y - 3, 6, 6);
        }

        void DrawHalfCircle(PaintEventArgs e, int radious, Point centre, float startAngle, float spanAngle)
        {
            Rectangle tempRec = new Rectangle(centre.X - radious, centre.Y - radious, 2 * radious, 2 * radious);
            e.Graphics.FillPie(Brushes.DarkOrange, tempRec, startAngle, spanAngle);
        }
        private Point getCircleCroxx(Point click, Point center, double radious)
        {
            int xsign = Math.Sign(click.X - center.X);
            int ysign = Math.Sign(click.Y - center.Y);
            Point transposedPoint = new Point(click.X - center.X, click.Y - center.Y);
            double k = (double)transposedPoint.Y / (double)transposedPoint.X;
            double x = (radious / (Math.Sqrt(1d + (k * k))));
            int y = (int)Math.Sqrt((radious * radious) - (x * x));
            x *= xsign;
            y *= ysign;
            return new Point((int)x + center.X, y + center.Y);
        }
        private double getCircleCroxxAngle(Point click, Point center, double radious)
        {
            int xsign = Math.Sign(click.X - center.X);
            int ysign = Math.Sign(click.Y - center.Y);
            Point transposedPoint = new Point(click.X - center.X, click.Y - center.Y);
            double k = (double)transposedPoint.Y / (double)transposedPoint.X;
            double angle = 180d * Math.Atan(k) / Math.PI;
            return angle;
        }

        int TransformValueWidth(int l, Rectangle r)
        {
            float xpos = r.X;
            float width = r.Width;
            float clickXPos = l;
            int value = (int)(180f * (clickXPos - xpos) / width);
            return value;
        }
        int TransformValueHeight(int l, Rectangle r)
        {
            float ypos = r.Y;
            float height = r.Height;
            float clickXPos = l;
            int value = (int)(180f * (1f - (clickXPos - ypos) / height));
            return value;
        }
    }


}

