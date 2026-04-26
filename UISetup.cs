using MelonLoader;
using UnityEngine;
using static BlindRumble2.Core;

namespace BlindRumble2
{
    public class UISetup
    {
        internal const string USER_DATA = "UserData/BlindRumble/";
        internal const string CONFIG_FILE = "config.cfg";

        internal static MelonPreferences_Category category1;
        internal static MelonPreferences_Category category2;
        internal static MelonPreferences_Entry<bool> enabledMod;
        internal static MelonPreferences_Entry<bool> enableInGym;
        internal static MelonPreferences_Entry<bool> enableInPark;
        internal static MelonPreferences_Entry<bool> enableInMatch;
        internal static MelonPreferences_Entry<string> MainColor;
        internal static MelonPreferences_Entry<string> SecondaryColor;

        public static void LoadPrefs()
        {
            if (!Directory.Exists(USER_DATA))
            {
                Directory.CreateDirectory(USER_DATA);
            }
                

            category1 = MelonPreferences.CreateCategory("Main");
            category1.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            category2 = MelonPreferences.CreateCategory("Colors");
            category2.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            enabledMod = category1.CreateEntry("enabledMod", true, "Enable Mod", "Enables the mod. Other settings wont work if this is disabled.");
            enableInGym = category1.CreateEntry("enableInGym", false, "Enable In Gym", "Enables Blind Rumble within Gym. Defaults to false.");
            enableInPark = category1.CreateEntry("enableInPark", true, "Enable In Park", "Enables Blind Rumble within Park. Defaults to true.");
            enableInMatch = category1.CreateEntry("enableInMatch", true, "Enable In Match", "Enables Blind Rumble within a match. Defaults to true.");
            MainColor = category2.CreateEntry("MainColor", "252, 216, 85, 1", "Main Color", "Color used for structures and players. Hex format.");
            SecondaryColor = category2.CreateEntry("SecondaryColor", "252, 216, 85, 1", "Secondary Color", "Color used for scene stuff. Hex format.");
        }

        public static void SetPrefs()
        {
            modEnabled = enabledMod.Value;
            EIGym = enableInGym.Value;
            EIPark = enableInPark.Value;
            EIMatch = enableInMatch.Value;
            if (!ColorUtility.TryParseHtmlString(MainColor.Value, out MainSonar)) loggerInstance.Error("Main Color did not save!");
            if (!ColorUtility.TryParseHtmlString(SecondaryColor.Value, out SecondarySonar)) loggerInstance.Error("Secondary Color did not save!");
        }
    }
}
