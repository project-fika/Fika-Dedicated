using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fika.Headless.Patches.DestroyGraphics
{
	public class DestroyGraphicsAutoloader
	{
		public static void EnableDestroyGraphicsPatches()
		{
			IEnumerable<Type> query = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(ModulePatch) && t.Namespace == "Fika.Headless.Patches.DestroyGraphics");

			FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("Autoloading patches in the DestroyGraphics namespace");
			int i = 0;

			foreach (Type patch in query)
			{
				if (patch.Name == "LoadScenePatch")
				{
					if (!FikaHeadlessPlugin.DestroyRenderersOnSceneLoad.Value)
					{
						FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo("DestroyRenderersOnSceneLoad is disabled! skipping LoadScenePatch");

						continue;
					}
				}

				((ModulePatch)Activator.CreateInstance(patch)).Enable();
				i++;
			}

			FikaHeadlessPlugin.FikaHeadlessLogger.LogInfo($"{i} Patches enabled");
		}
	}
}
