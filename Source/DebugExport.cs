using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.ModInterop;

namespace DebugMod
{
    [ModExportName(nameof(DebugMod))]
    public static class DebugExport
    {
        public static void AddActionToKeyBindList(Action method, string name, string category) 
            => DebugMod.AddActionToKeyBindList(method, name, category);

        public static void LogToConsole(string msg)
            => Console.AddLine(msg);
    }
}
