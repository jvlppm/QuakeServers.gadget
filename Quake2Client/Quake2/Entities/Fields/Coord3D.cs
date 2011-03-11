using System;

namespace Quake2.Entities.Fields
{
	public class Coord3D
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		public static Coord3D Parse(string value)
		{
			string[] values = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			Coord3D newCoord = new Coord3D();
			newCoord.X = double.Parse(values[0]);
			newCoord.Y = double.Parse(values[1]);
			newCoord.Z = double.Parse(values[2]);

			return newCoord;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", X, Y, Z);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode();
		}
	}
}