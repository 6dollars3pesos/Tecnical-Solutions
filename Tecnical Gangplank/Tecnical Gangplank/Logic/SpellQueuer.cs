using Aimtec;
using Spell = Aimtec.SDK.Spell;

namespace TecnicalGangplank.Logic
{
    public class SpellQueuer
    {
        private readonly Spell spell;
        private readonly Obj_AI_Base target;
        private readonly int expireTime;
        public SpellQueuer(Spell spell, Obj_AI_Base target, int expirationTickCount)
        {
            this.spell = spell;
            this.target = target;
            Game.OnUpdate += CastSpell;
            Obj_AI_Base.OnProcessSpellCast += SpellCastDetection;
            expireTime = expirationTickCount + Game.TickCount;
        }

        private void SpellCastDetection(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (sender.IsMe && e.SpellSlot == spell.Slot)
            {
                Game.OnUpdate -= CastSpell;
                Obj_AI_Base.OnProcessSpellCast -= SpellCastDetection;
            }
        }

        private void CastSpell()
        {
            if (!target.IsValid || Game.TickCount > expireTime)
            {
                Game.OnUpdate -= CastSpell;
                Obj_AI_Base.OnProcessSpellCast -= SpellCastDetection;
                return;
            }
            if (spell.Ready)
            {
                spell.Cast(target);
            }
        }
        
    }
}