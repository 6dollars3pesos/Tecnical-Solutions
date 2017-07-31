using Aimtec;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    public class TargetGetter
    {
        private readonly Config configuration;
        private readonly int staticRange;
        public TargetGetter(Config configuration, int staticRange)
        {
            this.configuration = configuration;
            this.staticRange = staticRange;
        }

        public Obj_AI_Hero getTarget(int range)
        {
            return Aimtec.SDK.TargetSelector.TargetSelector.Implementation.GetTarget(
                configuration.MiscDynamicTargetRange.Value ? range : staticRange);
        }
    }
}