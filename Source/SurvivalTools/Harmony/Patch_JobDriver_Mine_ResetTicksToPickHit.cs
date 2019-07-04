﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace SurvivalTools.Harmony
{

    [HarmonyPatch(typeof(JobDriver_Mine))]
    [HarmonyPatch("ResetTicksToPickHit")]
    public static class Patch_JobDriver_Mine_ResetTicksToPickHit
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand == AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.MiningSpeed)))
                {
                    instruction.operand = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.DiggingSpeed));
                }

                yield return instruction;
            }
        }

    }

}
