using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace VmsPlusNTC.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }
        public static string UserName //ใช้สำหรับเก็บค่า username
        {
            get
            {
                return AppSettings.GetValueOrDefault("Username", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("Username", value);
            }
        }
        public static string Password //ใช้สำหรับเก็บค่า username
        {
            get
            {
                return AppSettings.GetValueOrDefault("Password", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("Password", value);
            }
        }

        public static string Token 
        {
            get
            {
                return AppSettings.GetValueOrDefault("Token", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("Token", value);
            }
        }
        public static string Platform
        {
            get
            {
                return AppSettings.GetValueOrDefault("Platform", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("Platform", value);
            }
        }

        public static string InstallationId
        {
            get
            {
                return AppSettings.GetValueOrDefault("InstallationId", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("InstallationId", value);
            }
        }

        public static string tokenWebview
        {
            get
            {
                return AppSettings.GetValueOrDefault("tokenWebview", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("tokenWebview", value);
            }
        }
    }
}
