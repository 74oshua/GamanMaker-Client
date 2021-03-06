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
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg != null && pkg.Size() > 0)
            {
                UnityEngine.Debug.Log("changing weather...");
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

        public static void RPC_EventSetTime(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg != null && pkg.Size() > 0)
            {
                UnityEngine.Debug.Log("changing time of day...");
                String tod_str = pkg.ReadString();
                float tod = float.Parse(tod_str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                
                if (tod < 0f)
                {
                    EnvMan.instance.m_debugTimeOfDay = false;
                }
                else
                {
                    EnvMan.instance.m_debugTimeOfDay = true;
                    EnvMan.instance.m_debugTime = Mathf.Clamp01(tod);
                }
            }
        }

        public static void RPC_RequestSetTime(long sender, ZPackage pkg)
        {
            return;
        }

        public static void RPC_EventTestConnection(long sender, ZPackage pkg)
        {
            UnityEngine.Debug.Log("server has GamanMaker installed");
            GamanMaker.valid_server = true;
        }

        public static void RPC_RequestTestConnection(long sender, ZPackage pkg)
        {
            return;
        }

        public static void RPC_EventAdminSync(long sender, ZPackage pkg)
        {
            UnityEngine.Debug.Log("this account is an admin");
            GamanMaker.admin = true;
        }

        public static void RPC_RequestAdminSync(long sender, ZPackage pkg)
        {
            return;
        }

         public static void RPC_EventSetVisible(long sender, ZPackage pkg)
        {
            UnityEngine.Debug.Log("setting player visibility...");
            
            string name = pkg.ReadString();
            bool vis = pkg.ReadBool();

            if (!vis && !GamanMaker.invisible_players.Contains(name))
            {
                GamanMaker.invisible_players.Add(name);
            }
            else if (vis && GamanMaker.invisible_players.Contains(name))
            {
                GamanMaker.invisible_players.Remove(name);
            }
        }

        public static void RPC_RequestSetVisible(long sender, ZPackage pkg)
        {
            return;
        }
    }   
}