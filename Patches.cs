using HarmonyLib;
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
        internal static PLSectorInfo CometInitial;
        [HarmonyPatch(typeof(PLServer), "CPEI_HandleActivateWarpDrive")]
        internal class PLServerPatch1
        {
            static void Prefix()
            {
                if (PhotonNetwork.isMasterClient)
                {
                    //Syncing every jump to prevent floating point errors from deviating clients from server
                    PLSectorInfo CometSector = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET);
                    PLServer.Instance.photonView.RPC("ClientUpdateSectorData", PhotonTargets.Others, new object[] { PLServer.StarmapDataFromSector(CometSector) });
                }
            }
        }
        [HarmonyPatch(typeof(PLServer), "NetworkBeginWarp")]
        internal class PLServerPatch2
        {
            static void Prefix()
            {
                //Has the pathing update properly for the different campaigns/tutorial
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
                    if (CometInitial == null)
                    {
                        CometInitial = CometSector;
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
                    foreach (PLGalaxy.GalaxyGridCell gridCell in Traverse.Create(typeof(PLGlobal)).Field("Instance").Field("Galaxy").Field("GalaxyGridCells").GetValue<Dictionary<uint, PLGalaxy.GalaxyGridCell>>().Values)
                    {
                        if (gridCell.SectorsInGridCell.Contains(CometSector))
                        {
                            gridCell.SectorsInGridCell.Remove(CometSector);
                        }
                    }
                    CometSector.GridCell = null;
                    //forcing clients to use the most recent sync data
                    int AdjustmentFactor;
                    if (!PhotonNetwork.isMasterClient)
                    {
                        CometInitial = CometSector;
                        AdjustmentFactor = 1;
                    }
                    else
                    {
                        AdjustmentFactor = PLNetworkManager.Instance.NumJumps;
                    }
                    Vector2 distance = new Vector2(CometInitial.Position.x, CometInitial.Position.y);
                    float mag = distance.magnitude;
                    float initialAngle = Mathf.Deg2Rad * Mathf.Atan(distance.y / distance.x);
                    CometSector.Position = new Vector3(mag * Mathf.Cos(initialAngle + (2 * Mathf.PI * (AdjustmentFactor / 60f))), mag * Mathf.Sin(initialAngle + (2 * Mathf.PI * (AdjustmentFactor / 60f))), CometInitial.Position.z);
                    PLGlobal.Instance.Galaxy.GetGridCellForSector(CometSector, true);
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
