using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection.Emit;

namespace GamanMaker
{
    public class WeatherSystem
    {
        // code borrowed from https://github.com/Valheim-Modding/Wiki/wiki/Server-Validated-RPC-System and modified
        public static void RPC_EventSetWeather(long sender, ZPackage pkg)
        {
            UnityEngine.Debug.Log("changing weather...");
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg != null && pkg.Size() > 0)
            {
                String env_name = pkg.ReadString();

                if (env_name == "")
                {
                    EnvMan.instance.m_debugEnv = "";
                    return;
                }
                EnvMan.instance.m_debugEnv = env_name;
            }
        }

        public static void RPC_BadRequestMsg(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg != null && pkg.Size() > 0) { // Confirm our Server is sending the RPC
                string msg = pkg.ReadString(); // Get Our Msg
                if (msg != "")
                { // Make sure it isn't empty
                    Chat.instance.AddString("Server", "<color=\"red\">" + msg + "</color>", Talker.Type.Normal); // Add to chat with red color because it's an error
                }
            }
        }

        public static void RPC_RequestSetWeather(long sender, ZPackage pkg)
        {
            return;
        }
    }   
}