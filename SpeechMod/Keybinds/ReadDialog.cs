using HarmonyLib;
using Kingmaker;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM._PCView.Common;
using SpeechMod.Configuration.Settings;
using System;

#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Keybinds;

public class ReadDialog() : ModHotkeySettingEntry(_key, _title, _tooltip, _defaultValue)
{
    private const string _key = "dialog.read";
    private const string _title = "Read dialog";
    private const string _tooltip = "Read current dialog.";
    private const string _defaultValue = ";;All;false";
    private const string BIND_NAME = $"{Constants.SETTINGS_PREFIX}.newcontrols.ui.{_key}";

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        private static IDisposable _disposableBinding;

        [HarmonyPatch(typeof(CommonPCView), nameof(CommonPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void Add(CommonPCView __instance)
        {
#if DEBUG
            Debug.Log($"{nameof(CommonPCView)}_{nameof(CommonPCView.BindViewImplementation)}_Postfix : {BIND_NAME}");
#endif

            if (Game.Instance.Keyboard.m_Bindings.Exists(binding => binding.Name.Equals(BIND_NAME)))
            {
#if DEBUG
                Debug.Log($"Binding {BIND_NAME} already exists! Disposing of binding...");
#endif
                _disposableBinding.Dispose();
            }

            _disposableBinding = Game.Instance!.Keyboard!.Bind(BIND_NAME, delegate { ReadDialog(); });
            __instance?.AddDisposable(_disposableBinding);
        }

        private static void ReadDialog()
        {
            Main.Speech.SpeakDialog(Game.Instance?.DialogController?.CurrentCue?.DisplayText);
        }
    }
}