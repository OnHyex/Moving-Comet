using System;
using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;

//namespace Template
//{
//    internal class Command : ChatCommand
//    {
//        public override string[] CommandAliases()
//        {
//            return new string[] { "Sync" };
//        }
//        public override string[][] Arguments()
//        {
//            return new string[][] { new string[] { } };
//        }
//        public override string Description()
//        {
//            return "template command description";
//        }
//        public override void Execute(string arguments)
//        {
//            PLSectorInfo CometSector = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET);
//            PLServer.Instance.photonView.RPC("ClientUpdateSectorPosition", PhotonTargets.Others, new object[] { (short)CometSector.ID, CometSector.Position });
//        }
//    }
//    //internal class Command : ChatCommand
//    //{
//    //    public override string[] CommandAliases()
//    //    {
//    //        return new string[] { "Sync" };
//    //    }
//    //    public override string[][] Arguments()
//    //    {
//    //        return new string[][] { new string[] { } };
//    //    }
//    //    public override string Description()
//    //    {
//    //        return "template command description";
//    //    }
//    //    public override void Execute(string arguments)
//    //    {
//    //        PLSectorInfo CometSector = PLGlobal.Instance.Galaxy.GetSectorOfVisualIndication(ESectorVisualIndication.COMET);
//    //        PLServer.Instance.photonView.RPC("ClientUpdateSectorPosition", PhotonTargets.Others, new object[] { (short)CometSector.ID, CometSector.Position });
//    //    }
//    //}
//}

