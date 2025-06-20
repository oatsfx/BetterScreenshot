using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper;
using Il2CppAssets.Scripts.Models;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterScreenshot.Util
{
    public abstract class Utility : NamedModContent, IModSettings
    {
        protected virtual bool CreateCategory => false;

        protected virtual string Icon => TextureExists(Name) ? GetTextureGUID(Name) : null!;

        protected virtual string DisableIfModPresent => Name;

        protected static MelonLogger.Instance MelonLogger => GetInstance<BetterScreenshot>().LoggerInstance;

        public sealed override IEnumerable<ModContent> Load()
        {
            if (ModHelper.HasMod(DisableIfModPresent)) yield break;

            foreach (var nestedType in GetType().GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                mod.ApplyHarmonyPatches(nestedType);
            }

            OnLoad();
            yield return this;
        }

        public sealed override void Register()
        {
            BetterScreenshot.Utilities[Name] = this;

            if (CreateCategory)
            {
                var category = new ModSettingCategory(DisplayName)
                {
                    icon = Icon,
                    collapsed = true
                };
                foreach (var setting in mod.ModSettings.Values.Where(setting => setting.source == this))
                {
                    setting.category = category;
                }
            }

            OnRegister();
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnRegister()
        {
        }

        public virtual void OnSaveSettings()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnRestart()
        {
        }

        public virtual void OnNewGameModel(GameModel gameModel)
        {
        }
    }
}
