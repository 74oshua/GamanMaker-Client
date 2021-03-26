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
										if (ops[2] == "none")
										{
											GamanMaker.env_override.m_name = "";
											EnvMan.instance.m_debugEnv = "";
											break;
										}

										GamanMaker.env_override = EnvMan.instance.GetEnv(ops[2]);
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