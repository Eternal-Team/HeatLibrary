using BaseLibrary;
using Terraria.ModLoader;

namespace HeatLibrary
{
	public class HeatLibrary : Mod
	{
		internal static HeatLibrary Instance;

		public override void Load() => Instance = this;

		public override void Unload() => Utility.UnloadNullableTypes();
	}
}