using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Unity.Map;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Il2CppNinjaKiwi.Common;
using MelonLoader;
using System.Text.RegularExpressions;

namespace BetterScreenshot.Util;

public class MapScreenshoter : Utility
{
    //public static readonly ModSettingEnum<SeasonalEvent> EventSetting = new(SeasonalEvent.Normal)
    //{
    //    description = "Override and force a seasonal event to be active.",
    //};

    public static readonly ModSettingBool DisableEventProps = new(true)
    {
        icon = VanillaSprites.EventEasterLootIcon,
        description = "Disable the current event map props. Checked = no in-game event props",
    };

    public static bool IsUiEnabled = true;

    public static readonly ModSettingFolder FileLocation = new(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "BTD6_BetterScreenshot"))
    {
        icon = VanillaSprites.BlueInsertPanelRound,
        description = "Folder to save any screenshots made.",
        customValidation = Directory.Exists,
    };

    public static readonly ModSettingHotkey UiToggleHotkey = new(KeyCode.F11)
    {
        icon = VanillaSprites.HotkeysIcon,
        description = "Hotkey to push to toggle the in-game UI."
    };

    public static readonly ModSettingHotkey ScreenshotHotkey = new(KeyCode.F12)
    {
        icon = VanillaSprites.HotkeysIcon,
        description = "Hotkey to press that screenshots the map."
    };

    private static IEnumerable<GameObject?> InGameUIElements()
    {
        yield return InGame.instance?.mapRect?.gameObject;
        yield return MainHudLeftAlign.instance?.gameObject;
        yield return MainHudRightAlign.instance?.gameObject;
    }

    public override void OnUpdate()
    {
        if (UiToggleHotkey.JustPressed())
        {
            IsUiEnabled = !IsUiEnabled;
            ModHelper.Msg<BetterScreenshot>($"UI: {IsUiEnabled}");
        }

        if (InGame.instance != null)
        {
            if (ScreenshotHotkey.JustPressed())
            {
                MelonCoroutines.Start(Capture());
            }

            foreach (var element in InGameUIElements())
            {
                if (element == null) continue;

                var canvasGroup = element.GetComponentOrAdd<CanvasGroup>();
                canvasGroup.alpha = IsUiEnabled ? 1 : 0;
                canvasGroup.interactable = IsUiEnabled;

                canvasGroup.blocksRaycasts = IsUiEnabled ? true : (false || element.HasComponent<InGameMapRect>());
            }
        }
    }

    public System.Collections.IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame(); // Wait for UI to render
        if (InGame.instance != null)
        {
            var cam = InGame.instance.sceneCamera;
            var width = Screen.width;
            var height = Screen.height;

            int left = width * (int)cam.rect.x;
            int top = height * (int)cam.rect.y;
            int captureWidth = (int)(width * cam.rect.width) - 3 /* Buffer from side bar */;
            int captureHeight = (int)(height * cam.rect.height);

            MelonLogger.Msg($"Screenshotting bounds: {left}, {top}, {left + captureWidth}, {top + captureHeight}");

            var originalTargetTexture = cam.targetTexture;

            var renderTexture = new RenderTexture(width * (int)cam.rect.width, height * (int)cam.rect.height, 24);
            cam.targetTexture = renderTexture;

            cam.Render();

            var screenshot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);

            // Read the pixels from the RenderTexture
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(width * cam.rect.left, height * cam.rect.top, width * cam.rect.width, height * cam.rect.height), 0, 0);
            screenshot.Apply();

            // Reset RenderTexture
            cam.targetTexture = originalTargetTexture;
            RenderTexture.active = null;

            // Encode the captured texture to PNG format
            byte[] pngData = screenshot.EncodeToPNG();
            string basePath = System.IO.Path.Combine(FileLocation, $"{Regex.Replace(InGame.instance.MapDataSaveId, "(?<!^)([A-Z])", "_$1").ToLower()}.png");
            string path = basePath;
            int counter = 1;

            try
            {
                while (File.Exists(path))
                {
                    string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(basePath);
                    string ext = System.IO.Path.GetExtension(basePath);
                    path = System.IO.Path.Combine(FileLocation, $"{fileNameWithoutExt}_{counter}{ext}");
                    counter++;
                }
                File.WriteAllBytes(path, pngData);
                MelonLogger.Msg($"Saved camera screenshot to: {path}");
            }
            catch
            {
                MelonLogger.Warning($"Unable to save screenshot to {path}!");
            }

            Object.Destroy(renderTexture);
            Object.Destroy(screenshot);
        }
    }

    [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.CheckAndLoadHolidaySkins))]
    internal static class MapLoader_CheckAndLoadHolidaySkins
    {
        [HarmonyPrefix]
        internal static bool Prefix()
        {
            if (!DisableEventProps) return true;
            ModHelper.Msg<BetterScreenshot>($"Removing event objects...");
            return false;
        }
    }

    [HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive))]
    internal static class GameObject_SetActive
    {
        [HarmonyPrefix]
        internal static bool Prefix(GameObject __instance, bool value)
        {
            if (!DisableEventProps) return true;
            if (__instance.name.Contains("TrackArrow"))
            {
                __instance.active = false;
                return false;
            }
            return true;
        }
    }

    //public enum SeasonalEvent
    //{
    //    Normal,
    //    Easter,
    //    Anniversary,
    //    FourthOfJuly,
    //    Winter,
    //    Halloween
    //}
}
