using System;
using System.Reflection;
using System.Text;
using Modding;
#if CURRENTVERSION
using Modding.Delegates;
#endif
using UnityEngine;



namespace DebugMod.Compatibility
{
    public static class ReflectionHelper
    {
        public static TField GetField<TObject, TField>(TObject obj, string name)
        {
            #if CURRENTVERSION
            return Modding.ReflectionHelper.GetField<TObject, TField>(obj, name);
            #elif OLDVERSION

            return Modding.ReflectionHelper.GetAttr<TObject, TField>(obj, name);
            #endif
        }
        
        public static void SetField<TObject, TField>(TObject obj, string name, TField value)
        {
            #if CURRENTVERSION
            Modding.ReflectionHelper.SetField(obj,name,value);
            #elif OLDVERSION

            Modding.ReflectionHelper.SetAttr(obj,name,value);
            #endif
        }
    }
}