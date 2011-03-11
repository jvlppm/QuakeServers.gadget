using Quake2.Entities.Fields;

namespace Quake2.Entities
{
	public class TargetSpeaker : Entity
	{
		public TargetSpeaker()
		{
			KeyValues.Add("attenuation", default(float));
			KeyValues.Add("volume", default(float));
			KeyValues.Add("origin", default(Coord3D));
			KeyValues.Add("noise", default(string));
			KeyValues.Add("spawnflags", 1);
		}

		public enum PlayMode
		{
			NonLoopedStartoff = 0,
			LoopedStartOn = 1,
			LoopedStartOff = 2
		}

		public PlayMode Mode
		{
			get { return (PlayMode)(((uint)KeyValues["spawnflags"]) & 3); }
			set { KeyValues["spawnflags"] = ((uint)KeyValues["spawnflags"] & ~3) | (uint)value; }
		}

		public float Attenuation
		{
			get { return (float)KeyValues["attenuation"]; }
			set { KeyValues["attenuation"] = value; }
		}

		public float Volume
		{
			get { return (float)KeyValues["volume"]; }
			set { KeyValues["volume"] = value; }
		}

		public Coord3D Origin
		{
			get { return (Coord3D)KeyValues["origin"]; }
			set { KeyValues["origin"] = value; }
		}

		public string Noise
		{
			get { return (string)KeyValues["noise"]; }
			set { KeyValues["noise"] = value; }
		}

		public void Set(string name, string value)
		{
			switch (name)
			{
				case "origin": Origin = Coord3D.Parse(value); break;
				case "volume": Volume = float.Parse(value); break;

				case "noise": Noise = value; break;
				case "attenuation": Attenuation = float.Parse(value); break;

				case "spawnflags": KeyValues["spawnflags"] = uint.Parse(value); break;

				default:
					if (!KeyValues.ContainsKey(name))
						KeyValues.Add(name, value);
					else
						KeyValues[name] = value;
					break;
			}
		}
	}
}
