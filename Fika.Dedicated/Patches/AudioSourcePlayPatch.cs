using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fika.Dedicated.Patches
{
	public class AudioSourcePlayPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(AudioSource).GetMethods().Where(x => x.Name == "Play" && x.GetParameters().Length == 0).SingleOrDefault();
		}

		[PatchPrefix]
		public static bool Prefix()
		{
			return false;
		}
	}
}
