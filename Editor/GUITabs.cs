﻿using EFT.UI;
using SAIN.Editor.GUISections;
using SAIN.Helpers;
using SAIN.Plugin;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor
{
    public static class GUITabs
    {
        public static void CreateTabs(EEditorTab selectedTab)
        {
            EditTabsClass.BeginScrollView();
            switch (selectedTab)
            {
                case EEditorTab.Home:
                    Home(); break;

                case EEditorTab.GlobalSettings:
                    GlobalSettings(); break;

                case EEditorTab.BotSettings:
                    BotSettings(); break;

                case EEditorTab.Personalities:
                    Personality(); break;

                case EEditorTab.Advanced:
                    Advanced(); break;

                default: break;
            }
            EditTabsClass.EndScrollView();
        }

        public static void Home()
        {
            ModDetection.ModDetectionGUI();
            Space(5f);
            PresetSelection.Menu();
        }

        public static void GlobalSettings()
        {
            string toolTip = $"Apply Values set below to GlobalSettings. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name} folder";
            if (BuilderClass.SaveChanges(GlobalSettingsWereEdited, toolTip, 35))
            {
                SAINPlugin.LoadedPreset.ExportGlobalSettings();
            }

            BotSettingsEditor.ShowAllSettingsGUI(SAINPlugin.LoadedPreset.GlobalSettings, out bool newEdit);
            if (newEdit)
            {
                GlobalSettingsWereEdited = true;
            }
        }

        public static bool GlobalSettingsWereEdited;

        public static void BotSettings()
        {
            BeginArea(SAINEditor.OpenTabRect);
            BotSelectionClass.Menu();
            EndArea();
        }

        public static void Personality()
        {
            BotPersonalityEditor.PersonalityMenu();
        }

        public static void Advanced()
        {
            const int spacing = 4;

            bool oldValue = SAINEditor.AdvancedBotConfigs;
            SAINEditor.AdvancedBotConfigs = SAINEditor.AdvancedBotConfigs.GUIToggle("Advanced Bot Configs", "Edit at your own risk.", EUISoundType.MenuCheckBox, Height(40));
            if (oldValue != SAINEditor.AdvancedBotConfigs)
            {
                SettingsContainers.UpdateCache();
                PresetHandler.SaveEditorDefaults();
            }

            oldValue = SAINPlugin.DebugModeEnabled;
            SAINPlugin.DebugModeEnabled = SAINPlugin.DebugModeEnabled.GUIToggle("Global Debug Mode", EUISoundType.MenuCheckBox, Height(40));
            if (oldValue != SAINPlugin.DebugModeEnabled)
            {
                PresetHandler.SaveEditorDefaults();
            }

            oldValue = SAINPlugin.DrawDebugGizmos;
            SAINPlugin.DrawDebugGizmos = SAINPlugin.DrawDebugGizmos.GUIToggle("Draw Debug Gizmos", EUISoundType.MenuCheckBox, Height(40));
            if (oldValue != SAINPlugin.DrawDebugGizmos)
            {
                PresetHandler.SaveEditorDefaults();
            }

            Space(spacing);

            BeginHorizontal();
            Box("GUI Scaling Height", Width(200f), Height(30f));
            RectLayout.ConfigScalingHeight = BuilderClass.CreateSlider(RectLayout.ConfigScalingHeight, 1f, 4f, Height(30f));
            RectLayout.ConfigScalingHeight = (float)BuilderClass.ResultBox(RectLayout.ConfigScalingHeight, Width(100f), Height(30f));
            EndHorizontal();

            Space(spacing / 2f);

            BeginHorizontal();
            Box("GUI Scaling Width", Width(200f), Height(30f));
            RectLayout.ConfigScalingWidth = BuilderClass.CreateSlider(RectLayout.ConfigScalingWidth, 1f, 4f, Height(30f));
            RectLayout.ConfigScalingWidth = (float)BuilderClass.ResultBox(RectLayout.ConfigScalingWidth, Width(100f), Height(30f));
            EndHorizontal();

            Space(spacing);

            ForceDecisionOpen = BuilderClass.ExpandableMenu("Force SAIN Bot Decisions", ForceDecisionOpen);

            if (ForceDecisionOpen)
            {
                Space(spacing);

                ForceSoloOpen = BuilderClass.ExpandableMenu("Force Solo Decision", ForceSoloOpen);
                if (ForceSoloOpen)
                {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSoloDecision = SoloDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSoloDecision = BuilderClass.SelectionGrid(
                        SAINPlugin.ForceSoloDecision,
                        EnumValues.GetEnum<SoloDecision>());
                }

                Space(spacing);

                ForceSquadOpen = BuilderClass.ExpandableMenu("Force Squad Decision", ForceSquadOpen);
                if (ForceSquadOpen)
                {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSquadDecision = SquadDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSquadDecision =
                        BuilderClass.SelectionGrid(SAINPlugin.ForceSquadDecision,
                        EnumValues.GetEnum<SquadDecision>());
                }

                Space(spacing);

                ForceSelfOpen = BuilderClass.ExpandableMenu("Force Self Decision", ForceSelfOpen);
                if (ForceSelfOpen)
                {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSelfDecision = SelfDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSelfDecision = BuilderClass.SelectionGrid(
                        SAINPlugin.ForceSelfDecision,
                        EnumValues.GetEnum<SelfDecision>());
                }
            }
        }

        private static bool ForceDecisionOpen;
        private static bool ForceSoloOpen;
        private static bool ForceSquadOpen;
        private static bool ForceSelfOpen;
    }
}