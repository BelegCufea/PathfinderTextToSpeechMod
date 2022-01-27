﻿using HarmonyLib;
using Kingmaker;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM._VM.Dialog.Dialog;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(DialogVM), "HandleOnCueShow")]
public static class Dialog_Patch
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(DialogVM)}_HandleOnCueShow_Postfix");
#endif

        if (!Main.Settings.AutoPlay)
        {
#if DEBUG
            Debug.Log($"{nameof(DialogVM)}: AutoPlay is disabled!");
#endif
            return;
        }

        string key = Game.Instance?.DialogController?.CurrentCue?.Text?.Key;
        if (string.IsNullOrWhiteSpace(key))
            key = Game.Instance?.DialogController?.CurrentCue?.Text?.Shared?.String?.Key;

        if (string.IsNullOrWhiteSpace(key))
            return;

        // Don't play if the dialog is voice acted.
        if (!string.IsNullOrWhiteSpace(LocalizationManager.SoundPack?.GetText(key, false)))
            return;

        Main.Speech.Speak(Game.Instance?.DialogController?.CurrentCue?.DisplayText, 0.5f);
    }
}