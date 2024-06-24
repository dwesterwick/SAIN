﻿using EFT;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace
{
    public class PlayerSpawnTracker
    {
        public readonly PlayerDictionary AlivePlayers = new PlayerDictionary();

        public readonly Dictionary<string, Player> DeadPlayers = new Dictionary<string, Player>();

        public PlayerComponent GetPlayerComponent(string profileId) => AlivePlayers.GetPlayerComponent(profileId);

        public PlayerComponent FindClosestHumanPlayer(out float closestPlayerSqrMag, Vector3 targetPosition, out Player player)
        {
            PlayerComponent closestPlayer = null;
            closestPlayerSqrMag = float.MaxValue;
            player = null;

            foreach (var component in AlivePlayers.Values)
            {
                if (component != null &&
                    component.Player != null &&
                    !component.IsAI)
                {
                    float sqrMag = (component.Position - targetPosition).sqrMagnitude;
                    if (sqrMag < closestPlayerSqrMag)
                    {
                        player = component.Player;
                        closestPlayer = component;
                        closestPlayerSqrMag = sqrMag;
                    }
                }
            }
            return closestPlayer;
        }

        public Player FindClosestHumanPlayer(out float closestPlayerSqrMag, Vector3 targetPosition)
        {
            FindClosestHumanPlayer(out closestPlayerSqrMag, targetPosition, out Player player);
            return player;
        }

        private void addPlayer(IPlayer iPlayer)
        {
            if (iPlayer == null)
            {
                Logger.LogError($"Could not add PlayerComponent for Null IPlayer.");
                return;
            }

            string profileId = iPlayer.ProfileId;
            Player player = GetPlayer(profileId);
            if (player == null)
            {
                Logger.LogError($"Could not add PlayerComponent for Null Player. IPlayer: {iPlayer.Profile?.Nickname} : {profileId}");
                return;
            }

            if (AlivePlayers.TryRemove(profileId, out bool compDestroyed))
            {
                string playerInfo = $"{player.name} : {player.Profile?.Nickname} : {profileId}";
                Logger.LogWarning($"PlayerComponent already exists for Player: {playerInfo}");
                if (compDestroyed)
                {
                    Logger.LogWarning($"Destroyed old Component for: {playerInfo}");
                }
            }

            PlayerComponent component = player.gameObject.AddComponent<PlayerComponent>();
            if (component?.Init(iPlayer, player) == true)
            {
                component.Person.ActiveClass.OnPersonDeadOrDespawned += removePerson;
                AlivePlayers.Add(profileId, component);
            }
            else
            {
                Logger.LogError($"Init PlayerComponent Failed for {player.name} : {player.ProfileId}");
                Object.Destroy(component);
            }
        }

        private void removePerson(PersonClass person)
        {
            person.ActiveClass.OnPersonDeadOrDespawned -= removePerson;

            AlivePlayers.TryRemove(person.ProfileId, out _);

            if (!person.ActiveClass.IsAlive && 
                person.Player != null)
            {
                //SAINGameWorld.StartCoroutine(addDeadPlayer(person.Player));
            }
        }

        public Player GetPlayer(string profileId)
        {
            if (!profileId.IsNullOrEmpty())
            {
                return GameWorldInfo.GetAlivePlayer(profileId);
            }
            return null;
        }

        private IEnumerator addDeadPlayer(Player player)
        {
            yield return null;

            if (player != null && 
                !player.HealthController.IsAlive)
            {
                if (DeadPlayers.Count > _maxDeadTracked)
                {
                    DeadPlayers.Remove(DeadPlayers.First().Key);
                }
                DeadPlayers.Add(player.ProfileId, player);
            }
        }

        public PlayerSpawnTracker(GameWorldComponent sainGameWorld)
        {
            _sainGameWorld = sainGameWorld;
            sainGameWorld.GameWorld.OnPersonAdd += addPlayer;
        }

        public void Dispose()
        {
            var gameWorld = _sainGameWorld?.GameWorld;
            if (gameWorld != null)
            {
                gameWorld.OnPersonAdd -= addPlayer;
            }
            foreach (var player in AlivePlayers)
            {
                player.Value?.Dispose();
            }
            AlivePlayers.Clear();
        }

        private readonly GameWorldComponent _sainGameWorld;
        private const int _maxDeadTracked = 30;
    }

    public class PlayerDictionary : Dictionary<string, PlayerComponent>
    {
        public PlayerComponent GetPlayerComponent(string profileId)
        {
            if (!profileId.IsNullOrEmpty() &&
                this.TryGetValue(profileId, out PlayerComponent component))
            {
                return component;
            }
            return null;
        }

        public bool TryRemove(string id, out bool destroyedComponent)
        {
            destroyedComponent = false;
            if (id.IsNullOrEmpty())
            {
                return false;
            }
            if (this.TryGetValue(id, out PlayerComponent playerComponent))
            {
                if (playerComponent != null)
                {
                    destroyedComponent = true;
                    playerComponent.Dispose();
                }
                this.Remove(id);
                return true;
            }
            return false;
        }

        public void ClearNullPlayers()
        {
            foreach (KeyValuePair<string, PlayerComponent> kvp in this)
            {
                PlayerComponent component = kvp.Value;
                if (component == null ||
                    component.IPlayer == null ||
                    component.Player == null)
                {
                    _ids.Add(kvp.Key);
                    if (component.IPlayer != null)
                    {
                        Logger.LogWarning($"Removing {component.Player.name} from player dictionary");
                    }
                }
            }
            if (_ids.Count > 0)
            {
                Logger.LogWarning($"Removing {_ids.Count} null players");
                foreach (var id in _ids)
                {
                    TryRemove(id, out _);
                }
                _ids.Clear();
            }
        }

        private readonly List<string> _ids = new List<string>();
    }
}