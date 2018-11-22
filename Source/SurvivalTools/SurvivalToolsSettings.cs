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
        public static bool toolMapGen = true;
        public static bool toolLimit = true;
        public static float toolDegradationFactor = 1f;
        public static bool ToolDegradation => toolDegradationFactor > 0f;
        //public static bool toolDegradation = true;

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
            options.Gap();
            options.CheckboxLabeled("Settings_ToolMapGen".Translate(), ref toolMapGen, "Settings_ToolMapGen_Tooltip".Translate());
            options.Gap();
            options.CheckboxLabeled("Settings_ToolLimit".Translate(), ref toolLimit, "Settings_ToolLimit_Tooltip".Translate());
            options.Gap();
            options.AddLabeledSlider("Settings_ToolDegradationRate".Translate(), ref toolDegradationFactor, 0f, 2f,
                rightAlignedLabel: toolDegradationFactor.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor), roundTo: 0.01f);
            //options.CheckboxLabeled("Settings_ToolDegradation".Translate(), ref toolDegradation, "Settings_ToolDegradation_Tooltip".Translate());

            options.End();

            Mod.GetSettings<SurvivalToolsSettings>().Write();

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hardcoreMode, "hardcoreMode", false);
            Scribe_Values.Look(ref toolMapGen, "toolMapGen", true);
            Scribe_Values.Look(ref toolLimit, "toolLimit", true);
            Scribe_Values.Look(ref toolDegradationFactor, "toolDegradationFactor", 1f);
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
