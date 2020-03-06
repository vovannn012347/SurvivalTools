using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using HarmonyLib;

namespace SurvivalTools.HarmonyPatches
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("XeoNovaDan.SurvivalTools");
            Harmony.DEBUG = true;

            // Automatic patches
            Log.Message("Automatic patches", false);
            harmony.PatchAll(Assembly.GetExecutingAssembly()); 

            // Manual patches
            // Plants that obstruct construction zones
            var postfixHandleBlockingThingJob = new HarmonyMethod(patchType, nameof(Postfix_HandleBlockingThingJob));
            Log.Message("Postfix1", false);
            harmony.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.HandleBlockingThingJob)), postfix: postfixHandleBlockingThingJob);
            Log.Message("Postfix2", false);
            harmony.Patch(AccessTools.Method(typeof(RoofUtility), nameof(RoofUtility.HandleBlockingThingJob)), postfix: postfixHandleBlockingThingJob);
            Log.Message("Postfix3", false);
            // Mining JobDriver - ResetTicksToPickHit
            var transpileResetTicksToPickHit = new HarmonyMethod(patchType, nameof(Transpile_JobDriver_Mine_ResetTicksToPickHit));
            Log.Message("Findme1", false);
            harmony.Patch(AccessTools.Method(typeof(JobDriver_Mine), "ResetTicksToPickHit"), transpiler: transpileResetTicksToPickHit);
            Log.Message("Findme2", false);
            // Thanks Mehni!
            if (!ModCompatibilityCheck.OtherInventoryModsActive)
                Log.Message("Findme3", false);
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), transpiler: new HarmonyMethod(patchType, nameof(Transpile_FloatMenuMakerMad_AddHumanlikeOrders)));
            Log.Message("Findme4", false);
            // erdelf never fails to impress :)
            #region JobDriver Boilerplate
             harmony.Patch(typeof(RimWorld.JobDriver_PlantWork).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_PlantWork_MakeNewToils)));
            Log.Message("Findme5", false);
            var transpileMineMakeNewToils = new HarmonyMethod(patchType, nameof(Transpile_JobDriver_Mine_MakeNewToils));
            Log.Message("Findme6", false);
            harmony.Patch(typeof(JobDriver_Mine).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: transpileMineMakeNewToils);
            Log.Message("Findme7", false);
            harmony.Patch(typeof(JobDriver_ConstructFinishFrame).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_ConstructFinishFrame_MakeNewToils)));
            Log.Message("Findme8", false);
            harmony.Patch(typeof(JobDriver_Repair).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_Repair_MakeNewToils)));
            Log.Message("Findme9", false);
            harmony.Patch(AccessTools.Method(typeof(JobDriver_Deconstruct), "TickAction"),
                new HarmonyMethod(patchType, nameof(Prefix_JobDriver_Deconstruct_TickAction)));
            Log.Message("Findme10", false);
            harmony.Patch(typeof(JobDriver_AffectRoof).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_AffectRoof_MakeNewToils)));
            #endregion

            // Fluffy Breakdowns
            if (ModCompatibilityCheck.FluffyBreakdowns)  
            {
                var maintenanceDriver = GenTypes.GetTypeInAnyAssembly("Fluffy_Breakdowns.JobDriver_Maintenance", null);
                Log.Message("Findme11", false);
                if (maintenanceDriver != null && typeof(JobDriver).IsAssignableFrom(maintenanceDriver))
                {
                    harmony.Patch(maintenanceDriver.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).MinBy(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Count())
                            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                            transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_Maintenance_MakeNewToils)));
                }
                else
                    Log.Error("Survival Tools - Could not find Fluffy_Breakdowns.JobDriver_Maintenance type to patch");
            }

            // Quarry
            if (ModCompatibilityCheck.Quarry)
            {
                var quarryDriver = GenTypes.GetTypeInAnyAssembly("Quarry.JobDriver_MineQuarry", null);
                Log.Message("Findme12", false);
                if (quarryDriver != null && typeof(JobDriver).IsAssignableFrom(quarryDriver))
                {
                    harmony.Patch(AccessTools.Method(quarryDriver, "Mine"), postfix: new HarmonyMethod(patchType, nameof(Postfix_JobDriver_MineQuarry_Mine)));
                    harmony.Patch(AccessTools.Method(quarryDriver, "ResetTicksToPickHit"), transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_MineQuarry_ResetTicksToPickHit)));
                }
                else
                    Log.Error("Survival Tools - Could not find Quarry.JobDriver_MineQuarry type to patch");
            }

            // Turret Extensions
            if (ModCompatibilityCheck.TurretExtensions)
            {
                var turretExtensionsDriver = GenTypes.GetTypeInAnyAssembly("TurretExtensions.JobDriver_UpgradeTurret", null);
                Log.Message("Findme13", false);
                if (turretExtensionsDriver != null && typeof(JobDriver).IsAssignableFrom(turretExtensionsDriver))
                    harmony.Patch(AccessTools.Method(turretExtensionsDriver, "Upgrade"), postfix: new HarmonyMethod(patchType, nameof(Postfix_JobDriver_UpgradeTurret_Upgrade)));
                else
                    Log.Error("Survival Tools - Could not find TurretExtensions.JobDriver_UpgradeTurret type to patch");
            }

            // Combat Extended
            if (ModCompatibilityCheck.CombatExtended)
            {
                // Prevent tools from incorrectly being removed based on loadout
                var combatExtendedHoldTrackerExcessThingClass = GenTypes.GetTypeInAnyAssembly("CombatExtended.Utility_HoldTracker", null);
                Log.Message("FuckCE1", false);
                if (combatExtendedHoldTrackerExcessThingClass != null)
                    harmony.Patch(AccessTools.Method(combatExtendedHoldTrackerExcessThingClass, "GetExcessThing"), postfix: new HarmonyMethod(patchType, nameof(Postfix_CombatExtended_Utility_HoldTracker_GetExcessThing)));
                else
                    Log.Error("Survival Tools - Could not find CombatExtended.Utility_HoldTracker type to patch");

                // Prevent pawns from picking up excess tools with Combat Extended's CompInventory
                var combatExtendedCompInventory = GenTypes.GetTypeInAnyAssembly("CombatExtended.CompInventory", null);
                if (combatExtendedCompInventory != null)
                    harmony.Patch(AccessTools.Method(combatExtendedCompInventory, "CanFitInInventory"), postfix: new HarmonyMethod(patchType, nameof(Postfix_CombatExtended_CompInventory_CanFitInInventory)));
                else
                    Log.Error("Survival Tools - Could not find CombatExtended.CompInventory type to patch");
            }

            // Prison Labor
            if (ModCompatibilityCheck.PrisonLabor)
            {
                // Fix pickaxe degradation and use correct mining speed stat
                var prisonLabourMineJobDriver = GenTypes.GetTypeInAnyAssembly("PrisonLabor.JobDriver_Mine_Tweak", null);
                Log.Message("Findme14", false);
                if (prisonLabourMineJobDriver != null)
                {
                    harmony.Patch(AccessTools.Method(prisonLabourMineJobDriver, "ResetTicksToPickHit"), transpiler: transpileResetTicksToPickHit);
                    harmony.Patch(prisonLabourMineJobDriver.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).Last()
                        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                        transpiler: transpileMineMakeNewToils);
                }
                    
            }
        }

        public static void Postfix_HandleBlockingThingJob(ref Job __result, Pawn worker)
        {
            Log.Message("Findme 15", false);
            if (__result?.def == JobDefOf.CutPlant && __result.targetA.Thing.def.plant.IsTree)
            {
                if (worker.MeetsWorkGiverStatRequirements(ST_WorkGiverDefOf.FellTrees.GetModExtension<WorkGiverExtension>().requiredStats))
                    __result = new Job(ST_JobDefOf.FellTree, __result.targetA);
                else
                    __result = null;
            }
        }

        // Credit to goes Mehni for letting me use this. Thanks!
        public static IEnumerable<CodeInstruction> Transpile_FloatMenuMakerMad_AddHumanlikeOrders(IEnumerable<CodeInstruction> instructions)
        {
            Log.Message("Findme16", false);
            MethodInfo playerHome = AccessTools.Property(typeof(Map), nameof(Map.IsPlayerHome)).GetGetMethod();
            List<CodeInstruction> instructionList = instructions.ToList();

            //instructionList.RemoveRange(instructions.FirstIndexOf(ci => ci.operand == playerHome) - 3, 5);
            //return instructionList;

            bool patched = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (!patched && (instruction.operand as MethodInfo) == playerHome)
               // if (!patched && (instruction.operand == playerHome) // CE, Pick Up And Haul etc.
                //if (instructionList[i + 3].opcode == OpCodes.Callvirt && instruction.operand == playerHome)
                //if (instructionList[i + 3].operand == playerHome)
                {
                    {
                        instruction.opcode = OpCodes.Ldc_I4_0;
                        instruction.operand = null;
                        yield return instruction;
                        patched = true;
                    }
                    //    //{ instructionList[i + 5].labels = instruction.labels;}
                    //    instructionList.RemoveRange(i, 5);
                    //    patched = true;
                }
                yield return instruction;
            }
        }

        #region JobDriver Boilerplate

        // Using the transpiler-friendly overload
        private static MethodInfo TryDegradeTool =>
            AccessTools.Method(typeof(SurvivalToolUtility), nameof(SurvivalToolUtility.TryDegradeTool), new[] { typeof(Pawn), typeof(StatDef) });

        #region Transpile_JobDriver_PlantWork_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_PlantWork_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            var plantHarvestingSpeed = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.PlantHarvestingSpeed));
            Log.Message("Findme17", false);
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // actor
                    yield return new CodeInstruction(OpCodes.Ldsfld, plantHarvestingSpeed); // ST_StatDefOf.PlantHarvestingSpeed
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool); // TryDegradeTool(actor, ST_StatDefOf.PlantHarvestingSpeed)
                }

                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand as FieldInfo == AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.PlantWorkSpeed)))
                {
                    instruction.operand = plantHarvestingSpeed;
                }

                yield return instruction;
            }
        }
        #endregion

        #region JobDriver_Mine
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_Mine_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            Log.Message("Findme18", false);
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // actor
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.DiggingSpeed))); // ST_StatDefOf.DiggingSpeed
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool); // TryDegradeTool(actor, ST_StatDefOf.DiggingSpeed)
                }

                yield return instruction;
            }
        }

        public static IEnumerable<CodeInstruction> Transpile_JobDriver_Mine_ResetTicksToPickHit(IEnumerable<CodeInstruction> instructions)
        {
            Log.Message("Findme19", false);
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand as FieldInfo == AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.MiningSpeed)))
                {
                    instruction.operand = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.DiggingSpeed));
                }

                yield return instruction;
            }
        }
        #endregion

        #region Construction JobDrivers

        private static FieldInfo ConstructionSpeed =>
            AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.ConstructionSpeed));

        #region Transpile_JobDriver_ConstructFinishFrame_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_ConstructFinishFrame_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
                }

                yield return instruction;
            }
        }
        #endregion
        #region Transpile_JobDriver_Repair_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_Repair_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
                }

                yield return instruction;
            }
        }
        #endregion
        #region Prefix_JobDriver_Deconstruct_TickAction
        public static void Prefix_JobDriver_Deconstruct_TickAction(JobDriver_Deconstruct __instance)
        {
            SurvivalToolUtility.TryDegradeTool(__instance.pawn, StatDefOf.ConstructionSpeed);
        }
        #endregion
        #region Transpile_JobDriver_AffectRoof_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_AffectRoof_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done)
                {
                    yield return instruction;
                    yield return new CodeInstruction(instructionList[(i + 1)].opcode, instructionList[(i + 1)].operand);
                    yield return new CodeInstruction(instructionList[(i + 2)].opcode, instructionList[(i + 2)].operand);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    yield return new CodeInstruction(OpCodes.Call, TryDegradeTool);
                    done = true;
                }

                yield return instruction;
            }
        }
        #endregion

        #endregion

        #region Modded JobDrivers

        #region Transpile_JobDriver_Maintenance_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_Maintenance_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
                }

                yield return instruction;
            }
        }
        #endregion

        #region Patch JobDriver_MineQuarry
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_MineQuarry_ResetTicksToPickHit(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand as FieldInfo == AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.MiningSpeed)))
                {
                    instruction.operand = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.DiggingSpeed));
                }

                yield return instruction;
            }
        }

        public static void Postfix_JobDriver_MineQuarry_Mine(JobDriver __instance, Toil __result)
        {
            Action tickAction = __result.tickAction;
            Pawn pawn = __instance.pawn;
            __result.tickAction = () =>
            {
                SurvivalToolUtility.TryDegradeTool(pawn, ST_StatDefOf.DiggingSpeed);
                tickAction();
            };
            __result.defaultDuration = (int)Mathf.Clamp(3000f / pawn.GetStatValue(ST_StatDefOf.DiggingSpeed), 500f, 10000f);
        }
        #endregion

        #region Postfix_JobDriver_UpgradeTurret_Upgrade
        public static void Postfix_JobDriver_UpgradeTurret_Upgrade(JobDriver __instance, Toil __result)
        {
            Action tickAction = __result.tickAction;
            Pawn pawn = __instance.pawn;
            __result.tickAction = () =>
            {
                SurvivalToolUtility.TryDegradeTool(pawn, StatDefOf.ConstructionSpeed);
                tickAction();
            };
        }
        #endregion

        #endregion

        #endregion

        public static void Postfix_CombatExtended_Utility_HoldTracker_GetExcessThing(ref bool __result, Thing dropThing)
        {
            // If there's an excess thing to be dropped for automatic loadout fixing and that thing is a tool, don't treat it as an excess thing
            if (__result && dropThing as SurvivalTool != null)
                __result = false;
        }

        public static void Postfix_CombatExtended_CompInventory_CanFitInInventory(ThingComp __instance, ref bool __result, Thing thing, ref int count)
        {
            // If the pawn could normally take an item to inventory - check if that item's a tool and obeys the pawn's carrying capacity
            if (__result && thing is SurvivalTool tool)
            {
                var compParentPawn = __instance.parent as Pawn;
                if (compParentPawn != null && !compParentPawn.CanCarryAnyMoreSurvivalTools())
                {
                    count = 0;
                    __result = false;
                }
            }
        }

    }
}
