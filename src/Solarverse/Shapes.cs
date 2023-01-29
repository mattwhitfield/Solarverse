using System.Windows.Media;

namespace Solarverse
{
    internal static class Shapes
    {
        public static Geometry Circle { get; } = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
        public static Geometry UpTriangle { get; } = Geometry.Parse("M0,30L30,30 15,0Z");
        public static Geometry DownTriangle { get; } = Geometry.Parse("M0,0L30,0 15,30Z");
        public static Geometry DoubleDownTriangle { get; } = Geometry.Parse("M0,0L30,0 15,30ZM0,30L30,30 15,60Z");
    }
}
