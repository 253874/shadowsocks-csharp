﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Shadowsocks.Models;

namespace Shadowsocks
{
    public class SystemProxy
    {

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool _settingsReturn, _refreshReturn;

        public static void NotifyIE()
        {
            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            _settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            _refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        public static void Update(IConfig config, bool forceDisable)
        {
            bool global = config.global;
            bool enabled = config.enabled;
            if (forceDisable)
            {
                enabled = false;
            }
            try
            {
                RegistryKey registry =
                    Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings",
                        true);
                if (enabled)
                {
                    if (global)
                    {
                        registry.SetValue("ProxyEnable", 1);
                        registry.SetValue("ProxyServer", "127.0.0.1:" + config.localPort);
                        registry.SetValue("AutoConfigURL", "");
                    }
                    else
                    {
                        string pacUrl;
                        if (config.useOnlinePac && !string.IsNullOrEmpty(config.pacUrl))
                            pacUrl = config.pacUrl;
                        else
                            pacUrl = "http://127.0.0.1:" + config.localPort + "/pac?t=" + GetTimestamp(DateTime.Now);
                        registry.SetValue("ProxyEnable", 0);
                        var readProxyServer = registry.GetValue("ProxyServer");
                        registry.SetValue("ProxyServer", "");
                        registry.SetValue("AutoConfigURL", pacUrl);
                    }
                }
                else
                {
                    registry.SetValue("ProxyEnable", 0);
                    registry.SetValue("ProxyServer", "");
                    registry.SetValue("AutoConfigURL", "");
                }
                //Set AutoDetectProxy Off
                IEAutoDetectProxy(false);
                SystemProxy.NotifyIE();
                //Must Notify IE first, or the connections do not chanage
                CopyProxySettingFromLan();
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                // TODO this should be moved into views
                throw;
            }
        }

        private static void CopyProxySettingFromLan()
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections",
                    true);
            var defaultValue = registry.GetValue("DefaultConnectionSettings");
            try
            {
                var connections = registry.GetValueNames();
                foreach (String each in connections)
                {
                    if (!(each.Equals("DefaultConnectionSettings")
                        || each.Equals("LAN Connection")
                        || each.Equals("SavedLegacySettings")))
                    {
                        //set all the connections's proxy as the lan
                        registry.SetValue(each, defaultValue);
                    }
                }
                NotifyIE();
            }
            catch (IOException e)
            {
                Logging.LogUsefulException(e);
            }
        }

        private static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// Checks or unchecks the IE Options Connection setting of "Automatically detect Proxy"
        /// </summary>
        /// <param name="set">Provide 'true' if you want to check the 'Automatically detect Proxy' check box. To uncheck, pass 'false'</param>
        private static void IEAutoDetectProxy(bool set)
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections",
                    true);
            byte[] defConnection = (byte[])registry.GetValue("DefaultConnectionSettings");
            byte[] savedLegacySetting = (byte[])registry.GetValue("SavedLegacySettings");
            if (set)
            {
                defConnection[8] = Convert.ToByte(defConnection[8] & 8);
                savedLegacySetting[8] = Convert.ToByte(savedLegacySetting[8] & 8);
            }
            else
            {
                defConnection[8] = Convert.ToByte(defConnection[8] & ~8);
                savedLegacySetting[8] = Convert.ToByte(savedLegacySetting[8] & ~8);
            }
            registry.SetValue("DefaultConnectionSettings", defConnection);
            registry.SetValue("SavedLegacySettings", savedLegacySetting);
        }
    }
}
