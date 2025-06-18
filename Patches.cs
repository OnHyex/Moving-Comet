using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moving_Comet
{
    internal class Patches
    {
        private static Vector3 CalcNextPosition()
        {
            if (ModGUI.Mode < 5)
            {
                numJumps++;
                int AdjustmentFactor = numJumps;
                Vector2 distance = new Vector2(((Vector3)CometInitial).x, ((Vector3)CometInitial).y);
                float mag = distance.magnitude;
                float initialAngle = Mathf.Atan2(distance.y, distance.x);
                //60 is base divisor, each mode is slower than the previous non linearly, the mag multiplication is for it being faster than pure radial movement close in and slower further out to keep the speed more even on larger or smaller galaxy sizes
                float Divisor = 60f * (1f + 0.2f * Mathf.Pow(ModGUI.Mode,1.5f)) * mag;
                return new Vector3(mag * Mathf.Cos(initialAngle + (2 * Mathf.PI * (AdjustmentFactor / Divisor))), mag * Mathf.Sin(initialAngle + (2 * Mathf.PI * (AdjustmentFactor / Divisor))), ((Vector3)CometInitial).z);
            } 
            else
            {
                numJumps++;
                float GalaxyScale = PLGlobal.Instance.Galaxy.CalculateGenGalaxyScaleFromGenSettings();
                UnityEngine.Random.Range(GalaxyScale * -1, GalaxyScale);
                float radius = GalaxyScale * Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f));
                float theta = UnityEngine.Random.Range(0f, 1f) * 2 * Mathf.PI;
                return new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), ((Vector3)CometInitial).z);
            }
        }
        internal static object CometInitial;
        internal static int numJumps = 0;
        [HarmonyPatch(typeof(PLServer), "CPEI_HandleActivateWarpDrive")]
        internal class PLServerPatch1
        {
            static void Prefix()
            {
                //Messaging.Echo(PLNetworkManager.Instance.LocalPlayer, "CPEI_handle sync");
                PLSectorInfo CometSector = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET);
                if (CometInitial == null)
                {
                    CometInitial = CometSector.Position;
                }
                if (PhotonNetwork.isMasterClient)
                {

                    //if (!PhotonNetwork.isMasterClient)
                    //{
                    //    CometInitial = CometSector;
                    //}
                    CometSector.Position = CalcNextPosition();
                    PLServer.Instance.photonView.RPC("ClientUpdateSectorPosition", PhotonTargets.All, new object[] { (short)CometSector.ID, CometSector.Position });
                }     
            }
        }
        [HarmonyPatch(typeof(PLServer), "ClientUpdateSectorPosition")]
        internal class PLServerPatch3
        {
            static void Prefix(short id)
            {
                Mod.Instance.Disable();
                foreach (PhotonPlayer player in PulsarModLoader.MPModChecks.MPModCheckManager.Instance.NetworkedPeersWithMod(Mod.Instance.HarmonyIdentifier()))
                {
                    if (player.IsMasterClient)
                    {
                        Mod.Instance.Enable();
                        break;
                    }
                }
                if (Mod.Instance.IsEnabled() || PhotonNetwork.isMasterClient)
                {
                    //Messaging.Echo(PLNetworkManager.Instance.LocalPlayer, "client Recieved updated sector position");
                    //Syncing every jump to prevent floating point errors from deviating clients from server
                    PLShipInfo plshipInfo = PLEncounterManager.Instance.PlayerShip;
                    if (PLAcademyShipInfo.Instance != null)
                    {
                        plshipInfo = PLAcademyShipInfo.Instance;
                    }
                    if (PLAbyssShipInfo.Instance != null)
                    {
                        plshipInfo = null;
                    }
                    if (plshipInfo != null)
                    {
                        float neighborDistance = Mathf.Max(plshipInfo.MyStats.WarpRange, PLServer.Instance.GetCurrentHunterWarpRange()) * 1.1f;
                        PLSectorInfo CometSector = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET);
                        if (id != (short)CometSector.ID)
                        {
                            return;
                        }
                        if (CometInitial == null)
                        {
                            CometInitial = CometSector.Position;
                        }
                        //Remove old pathing data that is no longer valid could be optimized to find overlap with new comet position maybe or that might be worse
                        foreach (PLSectorInfo sector in CometSector.Neighbors)
                        {
                            if (sector.Neighbors.Contains(CometSector))
                            {
                                sector.Neighbors.Remove(CometSector);
                                PLGlobal.Instance.Galaxy.AllSectorInfos[sector.ID] = sector;
                            }
                        }
                        //removing comet from old gridcells for pathing data generation 
                        //uneeded as the method being patched includes a call of recalculategridcell which already does this
                        //foreach (PLGalaxy.GalaxyGridCell gridCell in Traverse.Create(typeof(PLGlobal)).Field("Instance").Field("Galaxy").Field("GalaxyGridCells").GetValue<Dictionary<uint, PLGalaxy.GalaxyGridCell>>().Values)
                        //{
                        //    if (gridCell.SectorsInGridCell.Contains(CometSector))
                        //    {
                        //        gridCell.SectorsInGridCell.Remove(CometSector);
                        //    }
                        //}
                        if (CometSector.GridCell != null)
                        {
                            PLGlobal.Instance.Galaxy.RecalculateGridCell(CometSector);
                        }
                        else
                        {
                            PLGlobal.Instance.Galaxy.GetGridCellForSector(CometSector, true);
                        }
                        //forcing clients to use the most recent sync data
                        CometSector.Neighbors = PLGlobal.Instance.Galaxy.GridSearch_FindSectorsWithinRange(CometSector.Position, neighborDistance * neighborDistance, CometSector);
                        foreach (PLSectorInfo sector in CometSector.Neighbors)
                        {
                            if (!sector.Neighbors.Contains(CometSector))
                            {
                                sector.Neighbors.Add(CometSector);
                                PLGlobal.Instance.Galaxy.AllSectorInfos[sector.ID] = sector;
                            }
                        }
                        PLGlobal.Instance.Galaxy.AllSectorInfos[CometSector.ID] = CometSector;
                    }
                }
            }
        }
        //[HarmonyPatch(typeof(PLServer), "NetworkBeginWarp")]
        //internal class PLServerPatch2
        //{
        //    static void Prefix()
        //    {
        //        Messaging.Echo(PLNetworkManager.Instance.LocalPlayer, "network begin warp");
        //        //Has the pathing update properly for the different campaigns/tutorial
                
        //    }
        //}
        //[HarmonyPatch(typeof(PLGalaxy), "Setup")] //Uneeded as during saving and loading the galaxy is saved and loaded from the previous state so I don't have to adjust it on load
        //internal class PLGalaxyPatch
        //{
        //    static void Postfix()
        //    {
        //        if (PhotonNetwork.isMasterClient)
        //        {
        //            PLSectorInfo CometSector = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET);
        //            if (CometInitial == null)
        //            {
        //                CometInitial = CometSector;
        //            }
        //            Vector2 distance = new Vector2(CometInitial.Position.x, CometInitial.Position.y);
        //            float mag = distance.magnitude;
        //            float initialAngle = Mathf.Deg2Rad * Mathf.Atan(distance.y / distance.x);
        //            CometSector.Position = new Vector3(mag * Mathf.Cos(initialAngle + (2 * Mathf.PI * (PLNetworkManager.Instance.NumJumps / 30f))), mag * Mathf.Sin(initialAngle + (2 * Mathf.PI * (PLNetworkManager.Instance.NumJumps / 30f))), Patches.CometInitial.Position.z);
        //            PLServer.Instance.photonView.RPC("ClientUpdateSectorData", PhotonTargets.All, new object[] { PLServer.StarmapDataFromSector(CometSector) });
        //        }
        //    }
        //}
    }
    
    

}
