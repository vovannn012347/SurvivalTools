using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SurvivalTools.HarmonyPatches
{

    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch(nameof(ThingDef.SpecialDisplayStats))]
        public static class Patch_ThingDef_SpecialDisplayStats
    {

        public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
        {
            // Tool def
            if (req.Thing == null && __instance.IsSurvivalTool(out SurvivalToolProperties tProps))
            {
                foreach (StatModifier modifier in tProps.baseWorkStatFactors)
                

                    __result = __result.AddItem(new StatDrawEntry(ST_StatCategoryDefOf.SurvivalTool,
                        modifier.stat.LabelCap,
                        modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                    /* 
                     * So this part I can't figure out, (this, modifer.stat) it Errors out. Something about not being able to convert from Ienumerable to errr, blah.   
                     */
                    //overrideReportTitle: SurvivalToolUtility.GetSurvivalToolOverrideReportText(this, modifier.stat), New, but still broken
                    //GetSurvivalToolOverrideReportText(this, modifier.stat),   Old
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
