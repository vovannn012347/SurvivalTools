﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using SettingsHelper;

namespace SurvivalTools
{
    public class SurvivalToolsSettings : ModSettings
    {

        public static bool hardcoreMode = false;

        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            options.Gap();
            // Same GUI colour as Merciless
            GUI.color = new Color(1f, 0.2f, 0.2f);
            options.CheckboxLabeled("Settings_HardcoreMode".Translate(), ref hardcoreMode, "Settings_HardcoreMode_Tooltip".Translate());
            GUI.color = defaultColor;

            options.End();

            Mod.GetSettings<SurvivalToolsSettings>().Write();

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hardcoreMode, "hardcoreMode", false);
        }

    }

    public class SurvivalTools : Mod
    {
        public SurvivalToolsSettings settings;

        public SurvivalTools(ModContentPack content) : base(content)
        {
            GetSettings<SurvivalToolsSettings>();
        }

        public override string SettingsCategory() => "SurvivalToolsSettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<SurvivalToolsSettings>().DoWindowContents(inRect);
        }
    }
}