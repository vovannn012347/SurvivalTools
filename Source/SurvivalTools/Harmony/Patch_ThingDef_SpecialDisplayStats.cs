using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.BaseGen;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace SurvivalTools.HarmonyPatches
{

    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch(nameof(ThingDef.SpecialDisplayStats))]
    //public static class Patch_ThingDef_SpecialDisplayStats
        public static class Patch_ThingDef_SpecialDisplayStats
    {

        public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
        {
            // Tool def
            if (req.Thing == null && __instance.IsSurvivalTool(out SurvivalToolProperties tProps))
            {
                foreach (StatModifier modifier in tProps.baseWorkStatFactors)
                    __result = __result.AddItem(new StatDrawEntry
                                                (ST_StatCategoryDefOf.SurvivalTool,
                                                modifier.stat.LabelCap,
                                                modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                                                //overrideReportTitle: SurvivalToolUtility.GetSurvivalToolOverrideReportText(this, modifier.stat),
                                                reportText: modifier.stat.description,
                                                displayPriorityWithinCategory: 99999));
            }

            // Stuff
            if (__instance.IsStuff && __instance.GetModExtension<StuffPropsTool>() is StuffPropsTool sPropsTool)
            {
                foreach (StatModifier modifier in sPropsTool.toolStatFactors)
                    __result = __result.AddItem(new StatDrawEntry(ST_StatCategoryDefOf.SurvivalToolMaterial,
                        modifier.stat.LabelCap,
                        modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                        reportText: modifier.stat.description,
                        displayPriorityWithinCategory: 99999));
            }
        }

    }

}
