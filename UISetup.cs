using MelonLoader;
using UIFramework;

namespace BlindRumble2
{
    public partial class Core
    {
        private const string USER_DATA = "UserData/BlindRumble/";
        private const string CONFIG_FILE = "config.cfg";

        private MelonPreferences_Category category1;
        private MelonPreferences_Category category2;
        private MelonPreferences_Entry<bool> enabledMod;
        private MelonPreferences_Entry<bool> enableInGym;
        private MelonPreferences_Entry<bool> enableInPark;
        private MelonPreferences_Entry<bool> enableInMatch;
        private MelonPreferences_Entry<string> MainColor;
        private MelonPreferences_Entry<string> SecondaryColor;

        public override void OnInitializeMelon()
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
            MainColor = category2.CreateEntry("MainColor", "252, 216, 85, 1", "Main Color", "Color used for structures and players. RGBA format.");
            SecondaryColor = category2.CreateEntry("SecondaryColor", "252, 216, 85, 1", "Secondary Color", "Color used for scene stuff. RGBA format.");

            UI.Register(this, category1, category2);

            modEnabled = enabledMod.Value;
            EIGym = enableInGym.Value;
            EIPark = enableInPark.Value;
            EIMatch = enableInMatch.Value;
            MainSonar = StringToColor(MainColor.Value);
            SecondarySonar = StringToColor(SecondaryColor.Value);
        }
    }
}
