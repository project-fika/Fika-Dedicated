using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace Fika.Headless.Patches
{
    public class MessageWindow_Show_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MessageWindow).GetMethod(nameof(MessageWindow.Show), [typeof(string),
                typeof(string),
                typeof(bool),
                typeof(float)]);
        }

        [PatchPostfix]
        public static GClass3546 PatchPostfix(GClass3546 __result, MessageWindow __instance)
        {
            __instance.Close(ECloseState.Accept);
            return __result;
        }
    }
}
