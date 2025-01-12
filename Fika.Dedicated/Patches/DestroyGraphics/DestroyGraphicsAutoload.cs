using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
    public class DestroyGraphicsAutoloader
    {
        public static void EnableDestroyGraphicsPatches()
        {
            IEnumerable<Type> query = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(ModulePatch) && t.Namespace == "Fika.Dedicated.Patches.DestroyGraphics");

            FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo("Autoloading patches in the DestroyGraphics namespace");
            int i = 0;

            foreach (Type patch in query)
            {
                if (patch.Name == "LoadScenePatch")
                {
                    if (!FikaDedicatedPlugin.DestroyRenderersOnSceneLoad.Value)
                    {
                        FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo("DestroyRenderersOnSceneLoad is disabled! skipping LoadScenePatch");

                        continue;
                    }
                }

                ((ModulePatch)Activator.CreateInstance(patch)).Enable();
                i++;
            }

            FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo($"{i} Patches enabled");
        }
    }
}
