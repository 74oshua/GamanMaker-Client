using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection.Emit;

namespace GamanMaker
{
	public class Patches
	{
		public class Game_Patch
		{
			[HarmonyPatch(typeof(Game), "Start")]
			[HarmonyPrefix]
			public static void Start_Prefix()
			{
				ZRoutedRpc.instance.Register("RequestSetWeather", new Action<long, ZPackage>(WeatherSystem.RPC_RequestSetWeather));
				ZRoutedRpc.instance.Register("EventSetWeather", new Action<long, ZPackage>(WeatherSystem.RPC_EventSetWeather));
				ZRoutedRpc.instance.Register("BadRequestMsg", new Action<long, ZPackage>(WeatherSystem.RPC_BadRequestMsg));
			}
		}

		public class EnemyHud_Patch
		{
			[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
			[HarmonyPrefix]
			public static bool ShowHud_Prefix()
			{
				return false;
			}
		}

		public class Console_Patch
		{
			[HarmonyPatch(typeof(Console), "InputText")]
			[HarmonyPrefix]
			public static void InputText_Patch(Console __instance)
			{
				String command = __instance.m_input.text;
				String[] ops = command.Split(' ');
				
				switch (ops[0])
				{
					case "weather":
						if (ops.Length > 1)
						{
							switch (ops[1])
							{
								case "list":
									__instance.AddString("availible environments:");
									String env_names = "none";
									foreach (EnvSetup env in EnvMan.instance.m_environments)
									{
										env_names += ", " + env.m_name;
									}
									__instance.AddString(env_names);
									break;
								case "set":
									if (ops.Length > 2)
									{
										ZPackage pkg = new ZPackage();
										if (ops[2] == "none")
										{
											pkg.Write("");
											ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSetWeather", new object[] { pkg });
											EnvMan.instance.m_debugEnv = "";
											break;
										}

										pkg.Write(ops[2]);
										ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSetWeather", new object[] { pkg });
										EnvMan.instance.m_debugEnv = ops[2];
									}
									else
									{
										__instance.AddString("Specify an environment name. Use 'weather list' to get a list of names");
									}
									break;
							}
						}
						else
						{
							__instance.AddString("Provide a weather command. Commands:");
							__instance.AddString("list 				- List all availible environment names");
							__instance.AddString("set <envname> 	- Set the current weather to that of the given environment by name");
						}
						break;
				}
			}
		}
	}
}