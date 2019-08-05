using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace HeatLibrary
{
	public class HeatLibraryConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(60)] [Range(5, 120)] [Label("$Mods.HeatLibrary.Config.DeltaCacheSize")] [Tooltip("$Mods.HeatLibrary.Config.DeltaCacheSizeTooltip")]
		public int DeltaCacheSize;
	}
}