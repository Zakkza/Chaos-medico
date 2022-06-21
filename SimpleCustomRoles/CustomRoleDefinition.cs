using System;
using System.ComponentModel;
using Exiled.CustomItems.API;
using ExiledFeatures = Exiled.API.Features;

namespace SCR
{
    public class CustomRoleDefinition
    {
        [Description("Whether or not this Role is Enabled")]
        public bool IsEnabled { get; set; } = true;

        [Description("The Name of the Role, this must be unique, this is used internally to keep track of roles and log to the console")]
        public string Name { get; set; } = "Default Custom Role Name";

        [Description("The Text for the Role's Badge")]
        public string BadgeText { get; set; } = "Default Custom Role";
        [Description("The Color for the Role's Badge")]
        public string BadgeColor { get; set; } = "silver";
        [Description("Whether or not the Badge should be Hidden")]
        public bool BadgeHidden { get; set; } = true;

        [Description("The Broadcast to show to the Player on Spawn")]
        public ExiledFeatures.Broadcast SpawnBroadcast { get; set; } = new ExiledFeatures.Broadcast(
            "You're a Default Custom Role.", 10, true, Broadcast.BroadcastFlags.Normal
        );

        [Description("The HP of the Role ( Default HP if < 0 )")]
        public float HP { get; set; } = -1.0f;

        [Description("The AHP of the Role ( Default AHP if < 0 )")]
        public float AHP { get; set; } = -1.0f;

        [Description("Whether or not AHP Decay is enabled ( This is only applied to effects given at spawn )")]
        public bool AHPDecay { get; set; } = true;

        [Description("The Items that the Players with this Role will Spawn With ( If no Item is specified then the Player's Inventory won't be Reset, use ItemType.None if you want to give an empty inventory )")]
        public ItemType[] SpawnItems { get; set; } = new ItemType[] { ItemType.None };

        [Description("The Base Roles to this Role, only Players with one of these can Spawn as this Role")]
        public RoleType[] BaseRoles { get; set; } = new RoleType[] { RoleType.None };

        [Description("The Chance for a Player to Spawn as this Role [0.0 - Infinity]")]
        public float SpawnChance { get; set; } = 0.0f;
        [Description("The Locations where this Role can Spawn, if multiple are given then a random one will be chosen")]
        public SpawnLocation[] SpawnLocations { get; set; } = new SpawnLocation[0];

        [Description("The Max amount of Players with this Role in a Match ( No Limit if < 0 )")]
        public int MaxPlayers { get; set; } = -1;
        [Description("The Min amount of Players that are Necessary for this Role to Spawn ( Always if < 0 )")]
        public int MinPlayersToSpawn { get; set; } = -1;

        public override string ToString()
        {
            return String.Format(
                "(Name='{0}',BadgeText='{1}',BadgeColor='{2}',SpawnBroadcast='{3}',AHP={4},SpawnItems=[{5}],BaseRoles=[{6}],SpawnChance={7},SpawnLocations=[{8}],MaxPlayers={9}MinPlayersToSpawn={10})",
                Name, BadgeText, BadgeColor, SpawnBroadcast.Content, AHP, String.Join(",", SpawnItems), String.Join(",", BaseRoles), SpawnChance, String.Join(",", SpawnLocations), MaxPlayers, MinPlayersToSpawn
            );
        }
    }
}
