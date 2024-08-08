using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.IO;

using GhostSpectator.Extensions;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using PluginAPI.Helpers;
using PluginAPI.Loader;
using System.Reflection;
using UnityEngine;

namespace GhostSpectator
{
    public class Plugin
    {
        [PluginPriority(LoadPriority.Medium)]
		[PluginEntryPoint(Translation.pluginName, "1.3.2", null, "Phineapple18")]
		public void OnLoad()
		{
            if (!pluginConfig.IsEnabled)
			{
                Log.Warning($"Plugin {Translation.pluginName} is disabled.", Translation.pluginName);
                return;
			}
            if (!AssemblyLoader.InstalledPlugins.Any(p => p.PluginName == "NWApiPermissionSystem"))
            {
                throw new DllNotFoundException($"The NWApiPermissionSystem plugin is required for {Translation.pluginName} plugin to work.");
            }
            if (File.Exists(Path.Combine(Paths.GlobalPlugins.Plugins, "0Harmony.dll")) || File.Exists(Path.Combine(Paths.LocalPlugins.Plugins, "0Harmony.dll")))
            {
                Log.Warning("The Harmony dependency should be placed in the dependencies folder.", Translation.pluginName);
            }
            Singleton = this;
            pluginHandler = PluginHandler.Get(this);
            EventManager.RegisterEvents<EventHandlers>(this);
            this.GetSpawnPoints(CultureInfo.InvariantCulture);
            this.CreateShootingRanges(CultureInfo.InvariantCulture);
            this.harmony = new($"{pluginHandler.PluginName.ToLower()}.{DateTime.UtcNow.Ticks}");
            this.harmony.PatchAll();
            setSize = AccessTools.Method(typeof(NetworkServer), "SendSpawnMessage", new[] { typeof(NetworkIdentity), typeof(NetworkConnection) });
            Log.Info($"Loaded plugin {pluginHandler.PluginName} by {pluginHandler.PluginAuthor}.", pluginHandler.PluginName);
        }

        private void GetSpawnPoints(CultureInfo format)
        {
            foreach (string data in pluginConfig.SpawnPositions)
            {
                try
                {
                    string[] position = data.Split(',');
                    OtherExtensions.SpawnPositions.Add(new(float.Parse(position[0], format), float.Parse(position[1], format), float.Parse(position[2], format)));
                    Log.Debug($"Loaded following ghost spawn position: {data}.", pluginConfig.Debug, pluginHandler.PluginName);
                }
                catch (Exception)
                {
                    Log.Debug($"The provided position {data} is in wrong format.", pluginConfig.Debug, pluginHandler.PluginName);
                    continue;
                }
            }
            if (OtherExtensions.SpawnPositions.Count == 0)
            {
                OtherExtensions.SpawnPositions.Add(new(9f, 1002f, 1f));
                Log.Error("All spawn positions are in wrong format, default spawn position will be used instead.", pluginHandler.PluginName);
                return;
            }
            Log.Debug($"Successfully loaded {OtherExtensions.SpawnPositions.Count} spawn positions for Ghosts.", pluginConfig.Debug, pluginHandler.PluginName);
        }

        private void CreateShootingRanges(CultureInfo format)
        {
            foreach (var kvp in pluginConfig.ShootingRanges)
            {
                string[] string1 = kvp.Key.Split(',');
                string[] string2 = kvp.Value.Split(',');

                Vector3 vector1;
                Vector3 vector2;
                try
                {
                    vector1 = new(float.Parse(string1[0], format), float.Parse(string1[1], format), float.Parse(string1[2], format));
                    vector2 = new(float.Parse(string2[0], format), float.Parse(string2[1], format), float.Parse(string2[2], format));
                }
                catch (Exception)
                {
                    Log.Debug($"The provided coordinates ({kvp.Key}) ({kvp.Value}) are in wrong format.", pluginConfig.Debug, pluginHandler.PluginName);
                    continue;
                }
                Bounds bounds = new((vector1 + vector2) / 2, (vector1 - vector2).Abs());
                OtherExtensions.ShootingRanges.Add(bounds);
                Log.Debug($"Created following shooting range bounds: {bounds}.", pluginConfig.Debug, pluginHandler.PluginName);
            }
            if (OtherExtensions.ShootingRanges.Count == 0)
            {
                OtherExtensions.ShootingRanges.Add(new(new(0f, 995.5f, -8f), new(20f, 1f, 8f)));
                Log.Error("All coordinates are in wrong format, default shooting range bounds will be used instead.", pluginHandler.PluginName);
                return;
            }
            Log.Debug($"Successfully created {OtherExtensions.ShootingRanges.Count} shooting ranges bounds.", pluginConfig.Debug, pluginHandler.PluginName);
        }

        private Harmony harmony;

        public PluginHandler pluginHandler;

        internal static MethodInfo setSize;

        [PluginConfig] public Config pluginConfig;

        [PluginConfig(Translation.translationFileName)] public Translation pluginTranslation;
       
        public static Plugin Singleton { get; private set; }
    }
}

