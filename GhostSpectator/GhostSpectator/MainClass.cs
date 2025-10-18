using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using UnityEngine;

namespace GhostSpectator
{
    public class MainClass : Plugin<Config>
    {
        public override void LoadConfigs()
        {
            pluginTranslation = this.LoadConfig<Translation>("translation.yml");
            base.LoadConfigs();
        }

        public override void Enable()
        {
            Instance = this;
            pluginConfig = Config;
            Events = new();
            CustomHandlersManager.RegisterEventsHandler(Events);
            this.CreateToySpawnRanges();
            if (pluginConfig.SsSettingsEnabled)
            {
                SSGhostSpectator.Singleton = new();
                SSGhostSpectator.Singleton.Enable();
            }
            harmony = new($"{Name.ToLower()}.{DateTime.UtcNow.Ticks}");
            harmony.PatchAll();
        }

        public override void Disable()
        {
            harmony.UnpatchAll();
            harmony = null;
            Toy.SpawnAreas = null;
            SSGhostSpectator.Singleton?.Disable();
            CustomHandlersManager.UnregisterEventsHandler(Events);
            Events = null;
            pluginConfig = null;
            Instance = null;
        }

        private void CreateToySpawnRanges()
        {
            foreach (ToyArea area in pluginConfig.ToySpawnAreas.ToList())
            {
                area.Bounds = new((area.Corner1 + area.Corner2) / 2, (area.Corner1 - area.Corner2).Abs());
                Toy.SpawnAreas.Add(area);
            }
        }

        public Config pluginConfig;
        public Translation pluginTranslation;
        private Harmony harmony;

        public EventHandler Events { get; private set; }
        public static MainClass Instance { get; private set; }

        public override string Author { get; } = "Catiatto";
        public override string Description { get; } = null;
        public override string Name { get; } = "GhostSpectator";
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        public override Version Version { get; } = new(3, 3, 0);
    }
}
