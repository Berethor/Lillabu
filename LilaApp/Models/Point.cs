namespace LilaApp.Models
{
    public struct Point
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Angle { get; set; }
        
        public Point(double x, double y, double angle = 0)
        {
            X = x;
            Y = y;
            Angle = angle;
        }
    }
}
