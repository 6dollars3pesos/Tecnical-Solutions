using System;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;
using TecnicalKatarina.Configurations;

namespace TecnicalKatarina.Logic
{
    public class IssueOrderBlocker
    {
        public bool Active;
        
        public IssueOrderBlocker()
        {
            Obj_AI_Base.OnIssueOrder += IssueOrder;
            SpellBook.OnCastSpell += CastSpell;
            Obj_AI_Base.OnProcessSpellCast += ProcessSpellCast;
        }

        private void ProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (!sender.IsMe || e.SpellSlot != SpellSlot.R ||
                !Storings.MenuConfiguration.MiscPreventRCancel.Value)
            {
                return;
            }
            Active = true;
            DelayAction.Queue(400, () => Active = false);
        }

        private void CastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs e)
        {
            if (sender.IsMe && Active && Storings.Player.HasBuff(Storings.RBUFFNAME))
            {
                e.Process = false;
            }
        }

        private void IssueOrder(Obj_AI_Base sender, Obj_AI_BaseIssueOrderEventArgs e)
        {
            if (Active && sender.IsMe && Storings.Player.HasBuff(Storings.RBUFFNAME))
            {
                e.ProcessEvent = false;
            }
        }
    }
}