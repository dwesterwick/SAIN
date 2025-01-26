using SPT.Reflection.Patching;
using Comfort.Common;
using Dissonance;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using HarmonyLib;
using SAIN.Helpers;
using SAIN.Layers;
using SAIN.Layers.Combat.Run;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SAIN
{
    public class BigBrainHandler
    {
        private static readonly string[] commonVanillaLayersToRemove = new string[]
        {
            "Help",
            "AdvAssaultTarget",
            "Hit",
            "Simple Target",
            "Pmc",
            "AssaultHaveEnemy",
            "Assault Building",
            "Enemy Building",
        };

        private static List<Type> _SAINLayers = new List<Type>();
        private static List<string> _SAINLayerNames = new List<string>();

        public static List<Type> SAINLayers
        {
            get
            {
                if (_SAINLayers.Count == 0)
                {
                    _SAINLayers.AddRange(typeof(SAINPlugin).Assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(SAINLayer))));
                }

                return _SAINLayers;
            }
        }

        public static List<string> SAINLayerNames
        {
            get
            {
                if (_SAINLayerNames.Count == 0)
                {
                    foreach (Type layerType in SAINLayers)
                    {
                        FieldInfo nameFieldInfo = layerType.GetField("Name", BindingFlags.Public | BindingFlags.Static);
                        if (nameFieldInfo == null)
                        {
                            Logger.LogError($"{layerType.Name} does not have a public static Name field. This is required for enabling vanilla layers!");
                        }
                        else
                        {
                            _SAINLayerNames.Add((string)nameFieldInfo.GetValue(null));
                        }
                    }
                }

                return _SAINLayerNames;
            }
        }

        public static void Init()
        {
            BrainAssignment.Init();
        }

        public static bool BigBrainInitialized;

        public class BrainAssignment
        {
            public static void Init()
            {
                addCustomLayersToPMCsAndRaiders();
                addCustomLayersToScavs();
                addCustomLayersToRogues();
                addCustomLayersToBloodHounds();
                addCustomLayersToBosses();
                addCustomLayersToFollowers();
                addCustomLayersToGoons();
                addCustomLayersToOthers();

                ToggleVanillaLayersForPMCBrains(new List<WildSpawnType>() { WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC }, false);
                ToggleVanillaLayersForOthers(false);
                ToggleVanillaLayersForAllBotBrains();
            }

            public static void ToggleVanillaLayersForAllBotBrains()
            {
                ToggleVanillaLayersForScavs(_vanillaBotSettings.VanillaScavs);
                ToggleVanillaLayersForRogues(_vanillaBotSettings.VanillaRogues);
                ToggleVanillaLayersForRaiders(_vanillaBotSettings.VanillaRaiders);
                ToggleVanillaLayersForBloodHounds(_vanillaBotSettings.VanillaBloodHounds);
                ToggleVanillaLayersForBosses(_vanillaBotSettings.VanillaBosses);
                ToggleVanillaLayersForFollowers(_vanillaBotSettings.VanillaFollowers);
                ToggleVanillaLayersForGoons(_vanillaBotSettings.VanillaGoons);
            }

            public static void ToggleVanillaLayersForPMCBrains(List<WildSpawnType> roles, bool useVanillaLayers)
            {
                List<string> brainList = new List<string>() { Brain.PMC.ToString() };


                List<string> LayersToToggle = new List<string>
                {
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "KnightFight",
                    //"PtrlBirdEye",
					"PmcBear",
                    "PmcUsec",
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, roles, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForRaiders(bool useVanillaLayers)
            {
                ToggleVanillaLayersForPMCBrains(new List<WildSpawnType>() { WildSpawnType.pmcBot }, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForScavs(bool useVanillaLayers)
            {
                List<string> brainList = getBrainList(AIBrains.Scavs);

                List<string> LayersToToggle = new List<string>
                {
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "PmcBear",
                    "PmcUsec",
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

                ToggleVanillaLayersForPMCBrains(new List<WildSpawnType>() { WildSpawnType.assaultGroup }, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForOthers(bool useVanillaLayers)
            {
                List<string> brainList = getBrainList(AIBrains.Others);

                List<string> LayersToToggle = new List<string>
                {
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "KnightFight",
                    //"PtrlBirdEye",
					"PmcBear",
                    "PmcUsec",
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForRogues(bool useVanillaLayers)
            {
                List<string> brainList = new List<string>() { Brain.ExUsec.ToString() };

                List<string> LayersToToggle = new List<string>
                {
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "KnightFight",
                    //"PtrlBirdEye",
					"PmcBear",
                    "PmcUsec",
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForBloodHounds(bool useVanillaLayers)
            {
                List<string> brainList = new List<string>() { Brain.ArenaFighter.ToString() };

                List<string> LayersToToggle = new List<string>
                {
                    "Request",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "KnightFight",
                    //"PtrlBirdEye",
					"PmcBear",
                    "PmcUsec",
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForBosses(bool useVanillaLayers)
            {
                List<string> brainList = getBrainList(AIBrains.Bosses);

                List<string> LayersToToggle = new List<string>
                {
                    "KnightFight",
                    "BirdEyeFight",
                    "BossBoarFight"
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForFollowers(bool useVanillaLayers)
            {
                List<string> brainList = getBrainList(AIBrains.Followers);

                List<string> LayersToToggle = new List<string>
                {
                    "KnightFight",
                    "BoarGrenadeDanger"
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
            }

            public static void ToggleVanillaLayersForGoons(bool useVanillaLayers)
            {
                List<string> brainList = getBrainList(AIBrains.Goons);

                List<string> LayersToToggle = new List<string>
                {
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "KnightFight",
                    "BirdEyeFight",
                    "Kill logic"
                };
                LayersToToggle.AddRange(commonVanillaLayersToRemove);

                toggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
            }

            private static void toggleVanillaLayers(List<string> brainNames, List<string> layerNames, bool useVanillaLayers)
            {
                if (useVanillaLayers)
                {
                    BrainManager.RemoveLayers(SAINLayerNames, brainNames);
                    BrainManager.RestoreLayers(layerNames, brainNames);
                }
                else
                {
                    checkExtractEnabled(layerNames);

                    BrainManager.RestoreLayers(SAINLayerNames, brainNames);
                    BrainManager.RemoveLayers(layerNames, brainNames);
                }
            }

            private static void toggleVanillaLayers(List<string> brainNames, List<string> layerNames, List<WildSpawnType> roles, bool useVanillaLayers)
            {
                if (useVanillaLayers)
                {
                    BrainManager.RemoveLayers(SAINLayerNames, brainNames, roles);
                    BrainManager.RestoreLayers(layerNames, brainNames, roles);
                }
                else
                {
                    checkExtractEnabled(layerNames);

                    BrainManager.RestoreLayers(SAINLayerNames, brainNames, roles);
                    BrainManager.RemoveLayers(layerNames, brainNames, roles);
                }
            }

            private static void addCustomLayersToPMCsAndRaiders()
            {
                addCustomLayersToPMCBrains(new List<WildSpawnType>() { WildSpawnType.pmcBot });
                addCustomLayersToPMCBrains(new List<WildSpawnType>() { WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC });
            }

            private static void addCustomLayersToPMCBrains(List<WildSpawnType> roles)
            {
                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
                List<string> pmcBrain = new List<string>() { Brain.PMC.ToString() };

                BrainManager.AddCustomLayer(typeof(DebugLayer), pmcBrain, 99, roles);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), pmcBrain, 80, roles);
                BrainManager.AddCustomLayer(typeof(ExtractLayer), pmcBrain, settings.SAINExtractLayerPriority, roles);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), pmcBrain, settings.SAINCombatSquadLayerPriority, roles);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), pmcBrain, settings.SAINCombatSoloLayerPriority, roles);
            }

            private static void addCustomLayersToScavs()
            {
                List<string> brainList = getBrainList(AIBrains.Scavs);
                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

                //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

                addCustomLayersToPMCBrains(new List<WildSpawnType>() { WildSpawnType.assaultGroup });
            }

            private static void addCustomLayersToOthers()
            {
                List<string> brainList = getBrainList(AIBrains.Others);

                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
                //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
            }

            private static void addCustomLayersToRogues()
            {
                List<string> brainList = new List<string>();
                brainList.Add(Brain.ExUsec.ToString());

                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
                //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
            }

            private static void addCustomLayersToBloodHounds()
            {
                List<string> brainList = new List<string>();
                brainList.Add(Brain.ArenaFighter.ToString());

                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
                //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
            }

            private static void addCustomLayersToBosses()
            {
                List<string> brainList = getBrainList(AIBrains.Bosses);

                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
                //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);
            }

            private static void addCustomLayersToFollowers()
            {
                List<string> brainList = getBrainList(AIBrains.Followers);

                var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
                //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);
            }

            private static void addCustomLayersToGoons()
            {
                List<string> brainList = getBrainList(AIBrains.Goons);

                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 64);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 62);
            }

            private static void checkExtractEnabled(List<string> layersToRemove)
            {
                if (GlobalSettingsClass.Instance.General.Extract.SAIN_EXTRACT_TOGGLE) {
                    layersToRemove.Add("Exfiltration");
                }
            }

            private static List<string> getBrainList(List<Brain> brains)
            {
                List<string> brainList = new List<string>();
                for (int i = 0; i < brains.Count; i++) {
                    brainList.Add(brains[i].ToString());
                }
                return brainList;
            }

            private static VanillaBotSettings _vanillaBotSettings => SAINPlugin.LoadedPreset.GlobalSettings.General.VanillaBots;
        }
    }
}