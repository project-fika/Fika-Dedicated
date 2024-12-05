using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using BSG.Unity.Wires;

namespace Fika.Dedicated.Patches.DestroyGraphics
{
	internal class LoadScenePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(SceneManager), nameof(SceneManager.LoadSceneAsync), [typeof(string), typeof(LoadSceneMode)]);
		}

		[PatchPostfix]
		static void Postfix(string sceneName, LoadSceneMode mode, AsyncOperation __result)
		{
			GameObject tempGameObject = new("SceneModificationHandler");
			SceneModificationHandler handler = tempGameObject.AddComponent<SceneModificationHandler>();

			handler.StartCoroutine(handler.WaitForSceneLoad(sceneName, __result));
		}
	}

	public class SceneModificationHandler : MonoBehaviour
	{
		public IEnumerator WaitForSceneLoad(string sceneName, AsyncOperation operation)
		{
			// Wait for the scene to finish loading
			while (!operation.isDone)
			{
				yield return null;
			}

			FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo($"Scene {sceneName} is fully loaded.");

			Scene loadedScene = SceneManager.GetSceneByName(sceneName);
			if (loadedScene.IsValid())
			{
				ModifyLoadedScene(loadedScene);
			}

			Destroy(gameObject);
		}

		private void ModifyLoadedScene(Scene scene)
		{
			foreach (GameObject RootGameObjects in scene.GetRootGameObjects())
			{
				//Logger.LogInfo($"Inspecting root object: {rootObj.name}");
				DestroyRenderers(RootGameObjects);
			}
		}

		private static readonly Type[] ProtectedComponents = { 
			typeof(AmbientLight),
			typeof(AreaLight),
			typeof(HotObject),
			typeof(RoadSplineGenerator),
			typeof(RoadSolidMarkGenerator),
			typeof(RoadMarksGenerator),
			typeof(WireGenerator), // These can't be removed, MeshRenderers keeps these alive at the moment
			typeof(WireSplineGenerator)
		};

		private void DestroyRenderers(GameObject prefab)
		{
			Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

			foreach (Renderer renderer in renderers)
			{
				// Check for protected components we absolutely cannot unload, these would break the game someway or another.
				bool hasProtectedComponent = false;
				foreach (Type componentType in ProtectedComponents)
				{
					if (renderer.gameObject.GetComponent(componentType) != null)
					{
						hasProtectedComponent = true;
						break;
					}
				}

				if (hasProtectedComponent)
				{
					continue;
				}

				// Unload materials and textures.
				foreach (Material material in renderer.sharedMaterials)
				{
					if (material != null)
					{
						if(material.HasProperty("_MainTex"))
						{
							material.mainTexture = null;
						}

						Destroy(material);
					}
				}

				// Destroy the renderer itself, note: This should not remove any colliders and such.
				FikaDedicatedPlugin.FikaDedicatedLogger.LogInfo($"Removing Renderer: {renderer.gameObject.name}");
				Destroy(renderer);
			}
		}
	}
}