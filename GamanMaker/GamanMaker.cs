using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GamanMaker
{
	[BepInPlugin("org.bepinex.plugins.gamanmaker", "GamanMaker-Client", "0.1.0.0")]
	public class GamanMaker : BaseUnityPlugin
	{

		public void Awake()
		{
			System.Console.WriteLine("Starting GamanMaker-Client");
			Harmony.CreateAndPatchAll(typeof(Patches.Game_Patch));
			Harmony.CreateAndPatchAll(typeof(Patches.Player_Patch));
			Harmony.CreateAndPatchAll(typeof(Patches.EnemyHud_Patch));
			Harmony.CreateAndPatchAll(typeof(Patches.Console_Patch));
		}

    	public static bool valid_server = false;
    	public static bool admin = false;
    	public static bool flying = false;
    	public static bool invisible = false;
	}
}
