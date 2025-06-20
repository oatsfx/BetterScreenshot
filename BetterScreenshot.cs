using MelonLoader;
using BTD_Mod_Helper;
using NoInGameUI;
using System.Collections.Generic;
using BetterScreenshot.Util;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using System.Linq;
using Il2CppAssets.Scripts.Data;

[assembly: MelonInfo(typeof(BetterScreenshot.BetterScreenshot), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BetterScreenshot;

public class BetterScreenshot : BloonsTD6Mod
{
    public static readonly Dictionary<string, Utility> Utilities = new();

    public static MelonPreferences_Category Preferences { get; private set; } = null!;

    public override void OnApplicationStart()
    {
        ModHelper.Msg<BetterScreenshot>("Thanks for using BetterScreenshot by oatsfx, I hope you find this mod useful.");
        Preferences = MelonPreferences.CreateCategory("BetterScreenshotPreferences");

        AccessTools.GetTypesFromAssembly(MelonAssembly.Assembly)
            .Where(type => !type.IsNested)
            .Do(ApplyHarmonyPatches);
    }

    public override void OnSaveSettings(JObject settings)
    {
        foreach (var utility in Utilities.Values)
        {
            utility.OnSaveSettings();
        }
    }

    public override void OnUpdate()
    {
        foreach (var utility in Utilities.Values)
        {
            utility.OnUpdate();
        }
    }

    public override void OnRestart()
    {
        foreach (var utility in Utilities.Values)
        {
            utility.OnRestart();
        }
    }

    public override void OnGameDataLoaded(GameData gameData)
    {

    }
}