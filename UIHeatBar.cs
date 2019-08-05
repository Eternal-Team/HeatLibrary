using BaseLibrary;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeatLibrary
{
	public class UIHeatBar : BaseElement
	{
		private IHeatHandler heatHandler;
		public HeatHandler Handler => heatHandler.HeatHandler;

		public UIHeatBar(IHeatHandler heatHandler)
		{
			this.heatHandler = heatHandler;
			GetHoverText += () => $"{Handler.Heat.ToSI("N0")}J/{Handler.Capacity.ToSI("N0")}J\n[c/{(Handler.AverageDelta >= 0 ? Color.Lime : Color.Red).ToHex()}:{Handler.AverageDelta.ToSI("N0")}W]";
		}

		// todo: actually draw the energy -> needs a shader
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			float progress = Handler.Heat / (float)Handler.Capacity;

			spriteBatch.DrawSlot(Dimensions);
		}
	}
}