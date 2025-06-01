using CodeStage.AntiCheat.ObscuredTypes;
using PulsarModLoader;
using PulsarModLoader.MPModChecks;
using System;

namespace Moving_Comet
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.0.0";
        public override string Author => "OnHyex";
        public override string ShortDescription => "template description";
        public override string Name => "Moving Comet";
        public override string HarmonyIdentifier()
        {
            return $"{Author}.{Name}";
        }
        public override int MPRequirements => (int)MPRequirement.All;
    }
}
