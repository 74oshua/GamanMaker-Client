using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GamanMaker
{
	[BepInPlugin("org.bepinex.plugins.gamanmaker", "GamanMaker", "0.1.0.0")]
	public class GamanMaker : BaseUnityPlugin
	{
		public void Awake()
		{
			System.Console.WriteLine("Starting GamanMaker");
			Harmony.CreateAndPatchAll(typeof(Patches.EnemyHud_Patch));
			Harmony.CreateAndPatchAll(typeof(Patches.Console_Patch));
		}

		public static EnvSetup env_override = new EnvSetup();
	}
}
