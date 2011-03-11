using Quake2.Entities.Fields;

namespace Quake2.Entities
{
	/*{
		"_sun_color" "248 225 186"
		"_sun_diffade" "1.25"
		"_sun_angle" "45 -60"
		"_sun_light" "700"
		"_sun_diffuse" "800"
		"_sun_ambient" "120"
		"sky" "bluesky"
		"message" "Inside The Rocks - By Sabotuer"
		"classname" "worldspawn"
	}*/

	public class WorldSpawn : Entity
	{
		public WorldSpawn()
		{
			KeyValues.Add("_sun_color", default(Color));
			KeyValues.Add("sky", default(string));
			KeyValues.Add("message", default(string));
		}

		public string Message
		{
			get { return (string)KeyValues["message"]; }
			set { KeyValues["message"] = value; }
		}
		
		public string Sky
		{
			get { return (string)KeyValues["sky"]; }
			set { KeyValues["sky"] = value; }
		}
		
		public Color SunColor
		{
			get { return (Color)KeyValues["_sun_color"]; }
			set { KeyValues["_sun_color"] = value; }
		}
		
		public void Set(string name, string value)
		{
			switch (name)
			{
				case "message": Message = value; break;
				case "sky": Sky = value; break;
				case "_sun_color": SunColor = Color.Parse(value); break;
				
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
