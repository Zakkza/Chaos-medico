using System;
using System.ComponentModel;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using ExiledFeatures = Exiled.API.Features;
using Exiled.API.Extensions;
using Exiled.API.Interfaces;
using Exiled.CustomItems.API;
using Exiled.Events.EventArgs;
using Handlers = Exiled.Events.Handlers;

namespace SCR
{
    public class SCRConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Enables Debug Log")]
        public bool EnableDebugLog { get; private set; } = true;

        [Description("Global Chance for a Base Role to spawn as a Custom one [0.0 - 1.0] (This is used for roles that aren't specified in CustomRoleChance)")]
        public float CustomRoleDefaultChance { get; private set; } = 0.5f;

        [Description("Chance for a Base Role to spawn as a Custom one [0.0 - 1.0]")]
        public Dictionary<RoleType, float> CustomRoleChance { get; private set; } = new Dictionary<RoleType, float>()
        {
            { RoleType.None     , 0.0f },
            { RoleType.Spectator, 0.0f },
            { RoleType.Tutorial , 0.0f }
        };

        [Description("The List of Custom Roles")]
        public CustomRoleDefinition[] CustomRoles { get; private set; } = new CustomRoleDefinition[]
        {
            new CustomRoleDefinition
            {
                Name = "Major Scientist",
                BadgeText = "Major Scientist",
                BadgeColor = "silver",
                BadgeHidden = true,
                SpawnBroadcast = new ExiledFeatures.Broadcast(
                    "You're a Major Scientist.", 10, true, Broadcast.BroadcastFlags.Normal
                ),
                HP = 200.0f,
                AHP = -1.0f,
                AHPDecay = true,
                SpawnItems = new ItemType[]
                {
                    ItemType.KeycardZoneManager, ItemType.ArmorLight, ItemType.Medkit, ItemType.GunCOM15
                },
                BaseRoles = new RoleType[]
                {
                    RoleType.Scientist
                },
                SpawnLocations = new SpawnLocation[]
                {
                    SpawnLocation.Inside173Armory
                },
                SpawnChance = 0.35f,
                MaxPlayers = 1,
                MinPlayersToSpawn = -1
            },
            new CustomRoleDefinition
            {
                Name = "Senior Guard",
                BadgeText = "Senior Guard",
                BadgeColor = "silver",
                BadgeHidden = true,
                SpawnBroadcast = new ExiledFeatures.Broadcast(
                    "You're a Senior Guard.", 10, true, Broadcast.BroadcastFlags.Normal
                ),
                HP = -1.0f,
                AHP = -1.0f,
                AHPDecay = true,
                SpawnItems = new ItemType[]
                {
                    ItemType.KeycardNTFOfficer, ItemType.ArmorCombat, ItemType.Medkit, ItemType.Painkillers, ItemType.GunE11SR, ItemType.GrenadeHE
                },
                BaseRoles = new RoleType[]
                {
                    RoleType.FacilityGuard
                },
                SpawnLocations = new SpawnLocation[0],
                SpawnChance = 0.25f,
                MaxPlayers = 1,
                MinPlayersToSpawn = -1
            }
        };
    }

    public class SCR : Plugin<SCRConfig>
    {
        public static SCR Instance { get; private set; } = new SCR();
        public override string Author => "hds536jhmk";
        public override string Name => "Simple Custom Roles";
        public override string Prefix => "SCR";
        public override Version Version => new Version(0, 8, 0);
        public override Version RequiredExiledVersion => new Version(5, 2, 0);
        public override PluginPriority Priority => PluginPriority.Default;

        private SCR() { }

        private Random RNG = new Random();
        private Dictionary<String, UserGroup> OldUserGroups = new Dictionary<String, UserGroup>();
        private Dictionary<String, int> CustomRoleCounts = new Dictionary<String, int>();

        public override void OnEnabled()
        {
            Log.Debug($"Loaded {Config.CustomRoles.Length} Custom Roles!", Config.EnableDebugLog);
            foreach (CustomRoleDefinition roleDefinition in Config.CustomRoles)
                Log.Debug(roleDefinition, Config.EnableDebugLog);

            Handlers::Server.RoundStarted += OnRoundStarted;
            Handlers::Player.ChangingRole += OnChangingRole;
            Handlers::Player.Spawning += OnSpawning;
            Handlers::Player.Died += OnDied;
            Log.Debug($"{Name} was Enabled!", Config.EnableDebugLog);
        }

        public override void OnDisabled()
        {
            Log.Debug($"{Name} was Disabled.", Config.EnableDebugLog);
            Handlers::Player.Died -= OnDied;
            Handlers::Player.Spawning -= OnSpawning;
            Handlers::Player.ChangingRole -= OnChangingRole;
            Handlers::Server.RoundStarted -= OnRoundStarted;
        }

        private void OnRoundStarted()
        {
            OldUserGroups.Clear();
            CustomRoleCounts.Clear();
        }

        private void RestoreOldUserGroup(Player p)
        {
            if (OldUserGroups.ContainsKey(p.UserId))
            {
                p.Group = OldUserGroups[p.UserId];
                OldUserGroups.Remove(p.UserId);
            }
        }

        private void SetPlayerRole(Player player, CustomRoleDefinition roleDefinition, out UnityEngine.Vector3 spawnPosition)
        {
            RestoreOldUserGroup(player);

            if (CustomRoleCounts.ContainsKey(roleDefinition.Name))
                CustomRoleCounts[roleDefinition.Name]++;
            else CustomRoleCounts[roleDefinition.Name] = 1;

            OldUserGroups.Add(player.UserId, player.Group?.Clone());

            UserGroup newUserGroup = player.Group?.Clone() ?? new UserGroup();
            newUserGroup.HiddenByDefault = roleDefinition.BadgeHidden;
            newUserGroup.BadgeText = roleDefinition.BadgeText;
            newUserGroup.BadgeColor = roleDefinition.BadgeColor;
            player.Group = newUserGroup;

            if (roleDefinition.SpawnLocations.Length > 0)
            {
                spawnPosition = SpawnExtensions.GetPosition(roleDefinition.SpawnLocations[
                    RNG.Next(0, roleDefinition.SpawnLocations.Length)
                ]);
            }
            else
            {
                spawnPosition = UnityEngine.Vector3.zero;
            }

            if (roleDefinition.HP >= 0.0f)
                player.Health = roleDefinition.HP;
            if (roleDefinition.AHP >= 0.0f)
                player.ArtificialHealth = roleDefinition.AHP;

            if (!roleDefinition.AHPDecay)
            {
                foreach (var process in player.ActiveArtificialHealthProcesses)
                    process.DecayRate = 0.0f;
            }

            if (roleDefinition.SpawnItems.Length > 0)
                player.ResetInventory(roleDefinition.SpawnItems);

            player.Broadcast(roleDefinition.SpawnBroadcast, false);
            
            Log.Debug(
                $"{player.Nickname} is one of the {CustomRoleCounts[roleDefinition.Name]} {roleDefinition.Name}.",
                Config.EnableDebugLog
            );
        }

        private int GetCustomRoleCount(CustomRoleDefinition roleDefinition)
        {
            return CustomRoleCounts.ContainsKey(roleDefinition.Name) ? CustomRoleCounts[roleDefinition.Name] : 0;
        }

        private void OnChangingRole(ChangingRoleEventArgs e)
        {
            RestoreOldUserGroup(e.Player);
        }

        private Tuple<List<CustomRoleDefinition>, double> GetCompatibleRoles(RoleType baseRole)
        {
            List<CustomRoleDefinition> compatibleRoles = new List<CustomRoleDefinition>();
            double totalSpawnChance = 0.0d;

            for (int i = 0; i < Config.CustomRoles.Length; i++)
            {
                CustomRoleDefinition roleDefinition = Config.CustomRoles[i];
                if (!roleDefinition.IsEnabled || roleDefinition.BaseRoles.Length == 0) continue;

                if ((roleDefinition.MinPlayersToSpawn < 0 || Server.PlayerCount >= roleDefinition.MinPlayersToSpawn) &&
                    (roleDefinition.MaxPlayers < 0 || GetCustomRoleCount(roleDefinition) < roleDefinition.MaxPlayers) &&
                    roleDefinition.BaseRoles.Contains(baseRole))
                {
                    compatibleRoles.Add(roleDefinition);
                    totalSpawnChance += roleDefinition.SpawnChance;
                }
            }

            return new Tuple<List<CustomRoleDefinition>, double>(compatibleRoles, totalSpawnChance);
        }

        private float GetRoleSpawnChance(RoleType role)
        {
            if (Config.CustomRoleChance.TryGetValue(role, out var chance))
                return chance;
            return Config.CustomRoleDefaultChance;
        }

        private void OnSpawning(SpawningEventArgs e)
        {
            if (RNG.NextDouble() > GetRoleSpawnChance(e.RoleType))
                return;

            var roles = GetCompatibleRoles(e.RoleType);
            if (roles.Item1.Count <= 0) return;

            double chanceToPick = RNG.NextDouble() * roles.Item2;
            double currentChance = 0.0d;

            for (int i = 0; i < roles.Item1.Count; i++)
            {
                CustomRoleDefinition roleDefinition = roles.Item1[i];
                currentChance += roleDefinition.SpawnChance;
                if (chanceToPick <= currentChance)
                {
                    SetPlayerRole(e.Player, roleDefinition, out var spawnPosition);
                    if (spawnPosition != UnityEngine.Vector3.zero)
                        e.Position = spawnPosition;
                    break;
                }
            }
        }

        private void OnDied(DiedEventArgs e)
        {
            RestoreOldUserGroup(e.Target);
        }
    }
}