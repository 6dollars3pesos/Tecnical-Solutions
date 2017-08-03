using Aimtec;
using Aimtec.SDK.TargetSelector;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    public class TargetGetter
    {
        private readonly int staticRange;
        public TargetGetter(int staticRange)
        {
            this.staticRange = staticRange;
        }

        public Obj_AI_Hero getTarget(int range)
        {
            return TargetSelector.Implementation.GetTarget(
                Storings.MenuConfiguration.MiscDynamicTargetRange.Value ? range : staticRange);
        }
    }
}