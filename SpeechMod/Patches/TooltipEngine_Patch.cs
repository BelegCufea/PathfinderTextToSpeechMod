﻿using HarmonyLib;
using Kingmaker.UI.MVVM._PCView.Tooltip.Bricks;
using Kingmaker.UI.MVVM._VM.Tooltip.Utils;
using System;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace SpeechMod
{
    [HarmonyPatch(typeof(TooltipEngine), "GetBrickView")]
    static class TooltipEngine_Patch
    {
        static void Postfix(ref MonoBehaviour __result)
        {
            if (!Main.Enabled)
                return;

            if (__result == null)
                return;

            // TODO: Possibly add more types
            if (__result is TooltipBrickTextView)
            {
                if (IsInvalid(__result.transform.parent))
                    return;

                var view = __result as TooltipBrickTextView;

                //Debug.Log("Getting child 'Text (TMP)'...");

                var textMeshProTransform = view.gameObject.transform.Find("Text (TMP)");
                if (textMeshProTransform == null)
                {
                    Debug.LogWarning("No TextMeshProUGUI found!");
                    return;
                }

                //Debug.Log("Getting component TextMeshPro");

                var textMeshPro = textMeshProTransform.GetComponent<TextMeshProUGUI>();
                if (textMeshPro == null)
                {
                    Debug.LogWarning("No TextMeshProUGUI found!");
                    return;
                }

                var skipEventAssignment = false;

                var defaultValues = textMeshProTransform.GetComponent<TextMeshProValues>();
                if (defaultValues == null)
                    defaultValues = textMeshProTransform.gameObject.AddComponent<TextMeshProValues>();
                else
                    skipEventAssignment = true;

                defaultValues.fontStyles = textMeshPro.fontStyle;
                defaultValues.color = textMeshPro.color;

                if (skipEventAssignment)
                {
                    //Debug.Log("Skipping event assignment!");
                    return;
                }

                var onPointerEnter = textMeshProTransform.GetComponent<ObservablePointerEnterTrigger>();
                var onPointerExit = textMeshProTransform.GetComponent<ObservablePointerExitTrigger>();
                var onPointerClick = textMeshProTransform.GetComponent<ObservablePointerClickTrigger>();
                if (onPointerEnter != null && onPointerExit != null && onPointerClick != null)
                {
                    onPointerEnter.OnPointerEnterAsObservable().Subscribe(_ =>
                    {
                        if(Main.Settings.FontStyleOnHover)
                            for (int i = 0; i < Main.Settings.FontStyles.Length; i++)
                            {
                                if (Main.Settings.FontStyles[i])
                                    textMeshPro.fontStyle |= (FontStyles)Enum.Parse(typeof(FontStyles), Main.FontStyleNames[i], true);
                            }
                        //textMeshPro.fontStyle |=  FontStyles.Underline;
                        if (Main.Settings.ColorOnHover)
                            textMeshPro.color = Main.ChosenColor;
                    });

                    onPointerExit.OnPointerExitAsObservable().Subscribe(_ =>
                    {
                        textMeshPro.fontStyle = defaultValues.fontStyles;
                        if (Main.Settings.ColorOnHover)
                            textMeshPro.color = defaultValues.color;
                    });

                    onPointerClick.OnPointerClickAsObservable().Subscribe(clickEvent =>
                    {
                        if (clickEvent.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
                            Speech.Speak(textMeshPro.text);
                    });
                }
            }
        }

        // TODO: Better way of telling if inside hover tooltip.
        private static bool IsInvalid(Transform parent)
        {
            return parent == null;
        }
    }
}