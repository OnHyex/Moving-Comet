using PulsarModLoader;
using PulsarModLoader.CustomGUI;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moving_Comet
{
    internal class ModGUI : ModSettingsMenu
    {
        public static SaveValue<int> Mode = new SaveValue<int>("MovingCometMode", 2);
        private static int ModeStorage = 0;
        private static string[] ModeNames = 
        {
            "Fastest", "Fast", "Normal",
            "Slow", "Slowest", "Random"
        };
        public override string Name()
        {
            return "Moving Comet";
        }
        public override void Draw()
        {
            GUILayout.BeginArea(new Rect(30, 30, 400, 400));
            GUILayout.BeginHorizontal();
            GUILayout.Box("Modes");
            GUILayout.EndHorizontal();
            Mode.Value = GUILayout.SelectionGrid(Mode.Value, ModeNames, 3);
            GUILayout.Label($"Mode: {ModeNames[Mode]}");
            if (PhotonNetwork.isMasterClient && PLServer.Instance != null && PLServer.GetCurrentSector().VisualIndication == ESectorVisualIndication.COMET)
            {
                if (GUILayout.Button("UnStuck"))
                {
                    PLServer.Instance.CPEI_HandleActivateWarpDrive(PLEncounterManager.Instance.PlayerShip.ShipID, PLServer.GetCurrentSector().ID, PLNetworkManager.Instance.LocalPlayer.GetPlayerID());
                }
            }
            GUILayout.EndArea();
            
        }
        public override void OnOpen()
        {
            base.OnOpen();
            ModeStorage = Mode.Value;
        }
        public override void OnClose()
        {
            base.OnClose();
            if (ModeStorage != Mode.Value)
            {
                if (PLGlobal.Instance != null && PLGlobal.Instance.Galaxy != null && PLGlobal.Instance.Galaxy.AllSectorInfos.Count > 0)
                {
                    //Messaging.Echo(PLNetworkManager.Instance.LocalPlayer, "Changing Start Sector");
                    Patches.CometInitial = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET).Position;
                    Patches.numJumps = 0;
                }
            }
        }
    }
}

