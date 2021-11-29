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
    } 
}
