﻿using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Fika.Headless.Patches
{
    // Token: 0x02000008 RID: 8
    internal class ErrorScreen_Show_Patch : ModulePatch
    {
        // Token: 0x06000017 RID: 23 RVA: 0x0000237C File Offset: 0x0000057C
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ErrorScreen).GetMethod("Show");
        }

        // Token: 0x06000018 RID: 24 RVA: 0x000023A4 File Offset: 0x000005A4
        [PatchPrefix]
        public static bool Prefix(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Logger.LogError("ErrorScreen.Show: " + message);
            }
            else
            {
                Logger.LogWarning("Received an empty error");
            }
            return false;
        }
    }
}
