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
				ZRoutedRpc.instance.Register("RequestSetTime", new Action<long, ZPackage>(WeatherSystem.RPC_RequestSetTime));
				ZRoutedRpc.instance.Register("EventSetTime", new Action<long, ZPackage>(WeatherSystem.RPC_EventSetTime));
				ZRoutedRpc.instance.Register("RequestTestConnection", new Action<long, ZPackage>(WeatherSystem.RPC_RequestTestConnection));
				ZRoutedRpc.instance.Register("EventTestConnection", new Action<long, ZPackage>(WeatherSystem.RPC_EventTestConnection));
				ZRoutedRpc.instance.Register("RequestAdminSync", new Action<long, ZPackage>(WeatherSystem.RPC_RequestAdminSync));
				ZRoutedRpc.instance.Register("EventAdminSync", new Action<long, ZPackage>(WeatherSystem.RPC_EventAdminSync));
				ZRoutedRpc.instance.Register("RequestSetVisible", new Action<long, ZPackage>(WeatherSystem.RPC_RequestSetVisible));
				ZRoutedRpc.instance.Register("EventSetVisible", new Action<long, ZPackage>(WeatherSystem.RPC_EventSetVisible));
				ZRoutedRpc.instance.Register("BadRequestMsg", new Action<long, ZPackage>(WeatherSystem.RPC_BadRequestMsg));
			}
		}

		public class ZNet
		{
			[HarmonyPatch(typeof(ZNet), "Shutdown")]
			[HarmonyPostfix]
			public static void Shutdown_Postfix()
			{
				GamanMaker.invisible_players.Clear();
			}
		}

		public class Player_Patch
		{
			[HarmonyPatch(typeof(Player), "Awake")]
			[HarmonyPostfix]
			public static void Awake_Postfix()
			{
				if (ZRoutedRpc.instance == null)
				{
					return;
				}
				ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSync", new object[] { new ZPackage() });
				ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestAdminSync", new object[] { new ZPackage() });
			}

			[HarmonyPatch(typeof(Player), "Update")]
			[HarmonyPrefix]
			public static void Update_Prefix(Player __instance)
			{
				__instance.SetVisible(true);
				foreach (var name in GamanMaker.invisible_players)
				{
					if (name == __instance.GetPlayerName())
					{
						__instance.SetVisible(false);
					}
				}
			}
		}

		public class Character_Patch
		{

			[HarmonyPatch(typeof(Character), "SetVisible")]
			[HarmonyPrefix]
			public static bool SetVisible_Prefix(Character __instance, bool visible)
			{
				if (visible)
				{
					__instance.m_lodGroup.localReferencePoint = __instance.m_originalLocalRef;
				}
				else
				{
					__instance.m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
				}
				return false;
			}
		}

		public class EnemyHud_Patch
		{
			[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
			[HarmonyPrefix]
			public static bool ShowHud_Prefix()
			{
				if (!GamanMaker.valid_server)
				{
					return true;
				}
				return false;
			}
		}

		public class Console_Patch
		{
			[HarmonyPatch(typeof(Console), "InputText")]
			[HarmonyPrefix]
			public static bool InputText_Patch(Console __instance)
			{
				if (!GamanMaker.valid_server || !GamanMaker.admin)
				{
					return true;
				}
				
				String command = __instance.m_input.text;
				String[] ops = command.Split(' ');
				
				ZPackage pkg = new ZPackage();
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
											pkg.Write("");
											ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSetWeather", new object[] { pkg });
                							// EnvMan.instance.m_debugEnv = "";
											break;
										}

										pkg.Write(ops[2]);
										ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSetWeather", new object[] { pkg });
										// EnvMan.instance.m_debugEnv = ops[2];
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
						return false;
					case "tod":
						if (ops.Length < 2)
						{
							return true;
						}
						
						pkg.Write(ops[1]);
						ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSetTime", new object[] { pkg });
						return false;
					case "sync":
						ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSync", new object[] { new ZPackage() });
						ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestAdminSync", new object[] { new ZPackage() });
						return false;
					case "invis":
						if (ops.Length < 2)
						{
							return true;
						}

						pkg.Write(Game.instance.GetPlayerProfile().m_playerName);
						switch (ops[1])
						{
							case "on":
								pkg.Write(false);
								break;
							case "off":
								pkg.Write(true);
								break;
						}
						
						ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestSetVisible", new object[] { pkg });
						return false;
				}
				return true;
			}
		}
	}
}