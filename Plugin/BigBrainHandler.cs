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
using System.Collections.Generic;
using System.Reflection;

namespace SAIN
{
    public class BigBrainHandler
    {
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

                ToggleVanillaLayersForPMCBrains(new List<WildSpawnType>() { WildSpawnType.pmcBot }, false);
                ToggleVanillaLayersForPMCBrains(new List<WildSpawnType>() { WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC }, false);
                ToggleVanillaLayersForOthers(false);
                ToggleVanillaLayersForAllBotBrains();
            }

            public static void ToggleVanillaLayersForAllBotBrains()
            {
                ToggleVanillaLayersForScavs(SAINEnabled.VanillaScavs);
                ToggleVanillaLayersForRogues(SAINEnabled.VanillaRogues);
                ToggleVanillaLayersForBloodHounds(SAINEnabled.VanillaBloodHounds);
                ToggleVanillaLayersForBosses(SAINEnabled.VanillaBosses);
                ToggleVanillaLayersForFollowers(SAINEnabled.VanillaFollowers);
                ToggleVanillaLayersForGoons(SAINEnabled.VanillaGoons);
            }

            public static void ToggleVanillaLayersForPMCBrains(List<WildSpawnType> roles, bool enabled)
            {
                List<string> brainList = new List<string>() { Brain.PMC.ToString() };

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    "FightReqNull",
                    "PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "PtrlBirdEye"
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList, roles);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList, roles);
                }
            }

            public static void ToggleVanillaLayersForScavs(bool enabled)
            {
                List<string> brainList = getBrainList(AIBrains.Scavs);

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "FightReqNull",
                    "PeacecReqNull",
                    "AssaultHaveEnemy",
                    "Assault Building",
                    "Enemy Building",
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
                }

                ToggleVanillaLayersForPMCBrains(new List<WildSpawnType>() { WildSpawnType.assaultGroup }, enabled);
            }

            public static void ToggleVanillaLayersForOthers(bool enabled)
            {
                List<string> brainList = getBrainList(AIBrains.Others);

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    "FightReqNull",
                    "PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "PtrlBirdEye"
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
                }
            }

            public static void ToggleVanillaLayersForRogues(bool enabled)
            {
                List<string> brainList = new List<string>() { Brain.ExUsec.ToString() };

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    "FightReqNull",
                    "PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "PtrlBirdEye"
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
                }
            }

            public static void ToggleVanillaLayersForBloodHounds(bool enabled)
            {
                List<string> brainList = new List<string>() { Brain.ArenaFighter.ToString() };

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Request",
                    "FightReqNull",
                    "PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
                }
            }

            public static void ToggleVanillaLayersForBosses(bool enabled)
            {
                List<string> brainList = getBrainList(AIBrains.Bosses);

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "BirdEyeFight",
                    "BossBoarFight"
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
                }
            }

            public static void ToggleVanillaLayersForFollowers(bool enabled)
            {
                List<string> brainList = getBrainList(AIBrains.Followers);

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "BoarGrenadeDanger"
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
                }
            }

            public static void ToggleVanillaLayersForGoons(bool enabled)
            {
                List<string> brainList = getBrainList(AIBrains.Goons);

                List<string> LayersToToggle = new List<string>
                {
                    "Help",
                    "AdvAssaultTarget",
                    "Hit",
                    "Simple Target",
                    "Pmc",
                    "AssaultHaveEnemy",
                    //"FightReqNull",
                    //"PeacecReqNull",
                    "Assault Building",
                    "Enemy Building",
                    "KnightFight",
                    "BirdEyeFight",
                    "Kill logic"
                };

                if (enabled)
                {
                    BrainManager.RestoreLayers(LayersToToggle, brainList);
                }
                else
                {
                    BrainManager.RemoveLayers(LayersToToggle, brainList);
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
                if (SAINEnabled.VanillaBosses)
                {
                    return;
                }

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
                if (SAINEnabled.VanillaFollowers)
                {
                    return;
                }

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
                if (SAINEnabled.VanillaGoons)
                {
                    return;
                }

                List<string> brainList = getBrainList(AIBrains.Goons);

                BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
                BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
                BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 64);
                BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 62);
            }

            private static List<string> getBrainList(List<Brain> brains)
            {
                List<string> brainList = new List<string>();
                for (int i = 0; i < brains.Count; i++)
                {
                    brainList.Add(brains[i].ToString());
                }
                return brainList;
            }

            private static VanillaBotSettings SAINEnabled => SAINPlugin.LoadedPreset.GlobalSettings.General.VanillaBots;
        }
    }
}