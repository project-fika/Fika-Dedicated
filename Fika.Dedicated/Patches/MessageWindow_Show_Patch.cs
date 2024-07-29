using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace Fika.Dedicated.Patches
{
    public class MessageWindow_Show_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MessageWindow).GetMethod(nameof(MessageWindow.Show), [typeof(string),
                typeof(string),
                typeof(Action),
                typeof(Action),
                typeof(float)]);
        }

        [PatchPostfix]
        static GClass3109 PatchPostfix(GClass3109 __result, MessageWindow __instance, string title, string message, Action acceptAction, Action cancelAction, float time)
        {
            __instance.Close(ECloseState.Accept);

            return __result;
        }
    }
}
