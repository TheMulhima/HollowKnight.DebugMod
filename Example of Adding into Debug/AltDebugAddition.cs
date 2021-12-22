using System;
using System.Linq;
using GlobalEnums;
using Modding;
using MonoMod.ModInterop;

namespace DebugAddition
{
    public class DebugAddition : Mod
    {
        public override string GetVersion() => "v1.0.0";

        public static DebugAddition Instance;

        public override void Initialize()
        {
            Instance = this;

            // The following line will be ignored if debug is not installed
            DebugMod.AddActionToKeyBindList(SomeMethod, "Do some logging", "DebugAddition methods");

            DebugMod.CreateSimpleInfoPanel("DebugAddition.SampleInfoPanel", 140);
            DebugMod.AddInfoToSimplePanel("DebugAddition.SampleInfoPanel", "Even Geo Count", () => DebugMod.GetStringForBool(PlayerData.instance.geo % 2 == 0));
            DebugMod.AddInfoToSimplePanel("DebugAddition.SampleInfoPanel", "Shade Geo", () => PlayerData.instance.geoPool.ToString());
        }

        public void SomeMethod()
        {
            Log("This goes to the modlog");
            DebugMod.LogToConsole("This goes to the debug console");
        }
    }
}
