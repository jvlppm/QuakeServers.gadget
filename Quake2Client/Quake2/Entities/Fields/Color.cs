using System;

namespace Quake2.Entities.Fields
{
	public class Color
	{
		public double Red { get; set; }
		public double Green { get; set; }
		public double Blue { get; set; }

		public static Color Parse(string value)
		{
			string[] values = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			Color newColor = new Color();
			newColor.Red = double.Parse(values[0]);
			newColor.Green = double.Parse(values[1]);
			newColor.Blue = double.Parse(values[2]);

			return newColor;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", Red, Green, Blue);
		}

		public override int GetHashCode()
		{
			return Red.GetHashCode();
		}
	}
}