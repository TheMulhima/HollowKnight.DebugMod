using System;
using System.Linq;
using GlobalEnums;
using Modding;
using MonoMod.ModInterop;

namespace DebugAddition
{
    public class DebugAddition:Mod
    {
        public override string GetVersion() => "v1.0.0";

        public static DebugAddition Instance;

        public override void Initialize()
        {
            Instance = this;
            typeof(DebugMod).ModInterop(); // If debug is installed, get the methods
            
            // The following line will be ignored if debug is not installed
            DebugMod.AddActionToKeyBindList(SomeMethod, "Do some logging", "DebugAddition methods");
        }
		
		    public void SomeMethod()
	    	{
    			Log("This goes to the modlog");
		    	DebugMod.LogToConsole("This goes to the debug console");
		    }
    }
    
    // We need to add this class - if debug is installed, MonoMod will automatically fill in the actions
    // when we call   typeof(DebugMod).ModInterop();
    [ModImportName("DebugMod")]
    internal static class DebugMod
    {
        public static Action<Action, string, string> AddActionToKeyBindList = null;
        public static Action<string> LogToConsole = null;
        public static Func<bool, string> GetStringForBool = null;
        public static Action<string, bool> CreateCustomInfoPanel = null;
        public static Action<string, float, float, float, string, Func<string>> AddInfoToPanel = null;
        public static Action<string, float> CreateSimpleInfoPanel = null;
        public static Action<string, string, Func<string>> AddInfoToSimplePanel = null;

    } 
}
