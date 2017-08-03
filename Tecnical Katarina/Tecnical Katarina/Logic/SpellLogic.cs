using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;
using TecnicalKatarina.Configurations;
using TecnicalKatarina.Extensions;
using Spell = Aimtec.SDK.Spell;

namespace TecnicalKatarina.Logic
{
    public static class SpellLogic
    {
        public static bool QSpell(bool harassMode = false)
        {
            Spell q = Storings.ChampionImpl.Q;
            if (!q.Ready || !harassMode && !Storings.MenuConfiguration.ComboQ.Value)
            {
                return false;
            }
            Obj_AI_Hero target = Storings.Selector.GetTarget(q.Range + 300);
            if (!harassMode && target != null 
                && Storings.MenuConfiguration.ComboQMinions.Value 
                && target.Distance(Storings.Player) > Storings.QMINIONDISTANCE + 100)
            {
                Vector3 optimalQPosition = target.Position.Extend(Storings.Player.Position, Storings.QMINIONDISTANCE);
                Obj_AI_Minion castMinion = GameObjects.EnemyMinions.MinBy(m => m.Distance(optimalQPosition));
                if (castMinion != null && castMinion.Distance(optimalQPosition) < 140)
                {
                    q.Cast(castMinion);
                    return true;
                }
            }
            target = Storings.Selector.GetTarget(q.Range);
            {
                if (target != null 
                    && Storings.MenuConfiguration.ComboQDirect.Value 
                    && (!Storings.MenuConfiguration.ComboQOnlyRunAway.Value 
                        || target.IsFacing(Storings.Player) 
                        || target.Distance(Storings.Player) < 300) 
                    || harassMode)
                {
                    q.Cast(target);
                    return true;
                }
            }
            return false;
        }

        public static bool WSpell()
        {
            if (Storings.MenuConfiguration.ComboW.Value && Storings.ChampionImpl.W.Ready
                && GameObjects.EnemyHeroes.Any(e => !e.IsDead && e.Distance(Storings.Player) <= 275))
            {
                Storings.ChampionImpl.W.Cast();
                return true;
            }
            return false;
        }

        public static bool ESpell(Obj_AI_Hero target = null, bool ksMode = false, bool harassMode = false)
        {
            var e = Storings.ChampionImpl.E;
            if (target == null
                && (Storings.MenuConfiguration.ComboE.Value || harassMode))
            {
                target = Storings.Selector.GetTarget(e.Range + 100);
            }
            if (target == null)
            {
                return false;
            }
            var eDaggers = Storings.DaggerManager.AllDaggers
                .Where(pos => pos.Distance(target) <= Storings.DAGGERTRIGGERRANGE + Storings.DAGGERDAMAGERANGE).ToList();
            if (eDaggers.Any())
            {
                Vector3 daggerPos = eDaggers.MinBy(pos => pos.Distance(target));
                Vector3 castPos = daggerPos.ReduceToMaxDistance(target.Position, Storings.DAGGERTRIGGERRANGE - 1);
                
                castPos = Storings.Player.Position.ReduceToMaxDistance(castPos, e.Range);
                if (castPos.Distance(target) < Storings.DAGGERDAMAGERANGE
                    && castPos.Distance(daggerPos) < Storings.DAGGERTRIGGERRANGE)
                {
                    e.Cast(castPos);
                    return true;
                }
                Vector2[] intersections = Helper.IntersectCircles(
                    Storings.Player.Position.To2D(), e.Range, daggerPos.To2D(), Storings.DAGGERTRIGGERRANGE);
                castPos = intersections.FirstOrDefault(pos => pos.Distance(target) < Storings.DAGGERDAMAGERANGE).To3D();
                if (castPos != default(Vector3))
                {
                    e.Cast(castPos);
                    return true;
                }
            }
            if ((Storings.MenuConfiguration.ComboEAlways.Value || ksMode) && !harassMode)
            {
                Vector3 castPos = Storings.Player.Position.ReduceToMaxDistance(target.Position, e.Range);
                if (castPos.Distance(target) < 50)
                {
                    e.Cast(castPos);
                    return true;
                }
            }
            return false;
        }

        public static bool RSpell()
        {
            Spell w = Storings.ChampionImpl.W;
            Spell e = Storings.ChampionImpl.E;
            Spell r = Storings.ChampionImpl.R;
            Obj_AI_Hero target = Storings.Selector.GetTarget(Storings.ChampionImpl.E.Range);
            {
                if (target != null && r.Ready && Storings.MenuConfiguration.ComboR.Value
                    && (target.HealthPercent() <= Storings.MenuConfiguration.ComboRHealth.Value
                        || Storings.Player.GetSpellDamage(target, SpellSlot.R) * 0.7 > target.Health)
                    && target.Distance(Storings.Player) < 
                    (e.Ready && Storings.MenuConfiguration.ComboE.Value ? e.Range : r.Range - 200))
                {
                    if (e.Ready && Storings.MenuConfiguration.ComboE.Value)
                    {
                        e.Cast(target.Position);
                        return true;
                    }
                    if (w.Ready && Storings.MenuConfiguration.ComboW.Value)
                    {
                        w.Cast(target.Position);
                        return true;
                    }
                    r.Cast();
                    return true;
                }
            }
            return false;
        }
    }
}