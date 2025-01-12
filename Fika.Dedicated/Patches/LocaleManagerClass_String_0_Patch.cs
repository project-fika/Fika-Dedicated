using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    internal class LocaleManagerClass_String_0_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocaleManagerClass).GetProperty(nameof(LocaleManagerClass.String_0)).GetSetMethod();
        }

        [PatchPrefix]
        public static bool Prefix(ref string ___string_2)
        {
            Logger.LogInfo("Forcing 'en' language");
            ___string_2 = "en";
            return false;
        }
    }
}
