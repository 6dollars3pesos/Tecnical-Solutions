using System;
using System.Drawing;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Util;
using Aimtec.SDK.Util.Cache;
using TecnicalKatarina.Configurations;
// ReSharper disable UnusedMethodReturnValue.Local

namespace TecnicalKatarina.Logic
{
    public class Katarina : Champion
    {
        private Obj_AI_Hero qTarget;
        private IssueOrderBlocker blocker = new IssueOrderBlocker();
        public Katarina() : base(new []{625f, 200f, 725f, 550f})
        {
        }

        public override void UpdateGame()
        {
            if (KillSteal())
            {
                return;
            }
            if (Storings.Player.HasBuff(Storings.RBUFFNAME))
            {
                Storings.MenuConfiguration.Orbwalker.AttackingEnabled = false;
                Storings.MenuConfiguration.Orbwalker.MovingEnabled = false;
            }
            else
            {
                Storings.MenuConfiguration.Orbwalker.AttackingEnabled = true;
                Storings.MenuConfiguration.Orbwalker.MovingEnabled = true;
            }
            switch (Storings.MenuConfiguration.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case OrbwalkingMode.Laneclear:
                    if (!JungleClear())
                    {
                        LaneClear();
                    }
                    break;
                case OrbwalkingMode.Lasthit:
                    LastHit();
                    break;
            }
        }


        public override void LoadGame()
        {
            AttackableUnit.OnDamage += OnDamage;
            Render.OnPresent += Draw;
        }
        
        private void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs e)
        {
            Obj_AI_Hero target = e.Target as Obj_AI_Hero;

            if (target != null && sender.IsMe
                && Math.Abs(e.Damage - Storings.Player.GetSpellDamage(target, SpellSlot.E)) < 5)
            {
                qTarget = null;
            }
        }


        protected override void ProcessPlayerCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            switch (e.SpellSlot)
            {
                case SpellSlot.Q:
                    Obj_AI_Hero hero = e.Target as Obj_AI_Hero;
                    if (hero != null)
                    {
                        qTarget = hero;
                        DelayAction.Queue(1200, () => qTarget = null);
                    }
                    break;
            }
        }

        protected override void ProcessPlayerAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
        }

        private bool Combo()
        {
            //Will try all the Spells until a spell actually casts
            //Beautiful Bash Scripting Style
            return SpellLogic.QSpell() || SpellLogic.WSpell() || SpellLogic.ESpell() || SpellLogic.RSpell();
        }
        
        private bool Harass()
        {
            if (Storings.MenuConfiguration.HarassQ.Value && SpellLogic.QSpell(true))
            {
                return true;
            }
            if (Storings.MenuConfiguration.HarassW.Value && SpellLogic.WSpell())
            {
                return true;
            }
            return Storings.MenuConfiguration.HarassE.Value && SpellLogic.ESpell(null, false, true);
        }

        private bool LaneClear()
        {
            if (Storings.MenuConfiguration.LaneClearQ.Value && Q.Ready)
            {
                Obj_AI_Base defaultMinion =
                    GameObjects.EnemyMinions.FirstOrDefault(m => m.Team == GameObjectTeam.Neutral
                                                            && m.IsValid
                                                            && m.Distance(Storings.Player) < Q.Range);
                if (defaultMinion != null)
                {
                    Q.Cast(defaultMinion);
                    return true;
                }
            }
            if (Storings.MenuConfiguration.LaneClearW.Value && W.Ready)
            {
                if (GameObjects.EnemyMinions.Any(m => m.Team == GameObjectTeam.Neutral
                                                 && m.IsValid
                                                 && m.Distance(Storings.Player) < Storings.DAGGERDAMAGERANGE))
                {
                    W.Cast();
                    return true;
                }
            }
            if (Storings.MenuConfiguration.LaneClearE.Value && E.Ready)
            {
                Obj_AI_Base defaultMinion =
                    GameObjects.EnemyMinions.FirstOrDefault(m => m.Team == GameObjectTeam.Neutral
                                                            && m.IsValid
                                                            && m.Distance(Storings.Player) < E.Range);
                if (defaultMinion != null)
                {
                    E.Cast(defaultMinion.Position);
                    return true;
                }
            }
            return false;
        }

        private bool JungleClear()
        {
            if (Storings.MenuConfiguration.JungleClearQ.Value && Q.Ready)
            {
                Obj_AI_Base defaultMinion =
                    GameObjects.Minions.FirstOrDefault(m => m.Team == GameObjectTeam.Neutral
                                                            && m.IsValid
                                                            && m.Distance(Storings.Player) < Q.Range);
                if (defaultMinion != null)
                {
                    Q.Cast(defaultMinion);
                    return true;
                }
            }
            if (Storings.MenuConfiguration.JungleClearW.Value && W.Ready)
            {
                if (GameObjects.Minions.Any(m => m.Team == GameObjectTeam.Neutral
                                                 && m.IsValid
                                                 && m.Distance(Storings.Player) < Storings.DAGGERDAMAGERANGE))
                {
                    W.Cast();
                    return true;
                }
            }
            if (Storings.MenuConfiguration.JungleClearE.Value && E.Ready)
            {
                Obj_AI_Base defaultMinion =
                    GameObjects.Minions.FirstOrDefault(m => m.Team == GameObjectTeam.Neutral
                                                            && m.IsValid
                                                            && m.Distance(Storings.Player) < E.Range);
                if (defaultMinion != null)
                {
                    E.Cast(defaultMinion.Position);
                    return true;
                }
            }
            return false;
        }

        private bool LastHit()
        {
            if (Q.Ready && Storings.MenuConfiguration.LastHitQ.Value)
            {
                Obj_AI_Minion minion = GameObjects.EnemyMinions.FirstOrDefault(
                    m => m.Distance(Storings.Player.Position) < Q.Range
                         && Storings.Player.GetSpellDamage(m, SpellSlot.Q) < m.Health);
                if (minion != null)
                {
                    Q.Cast(minion);
                    return true;
                }
            }
            if (E.Ready && Storings.MenuConfiguration.LastHitE.Value)
            {
                Obj_AI_Minion minion = GameObjects.EnemyMinions.FirstOrDefault(
                    m => m.Distance(Storings.Player.Position) < E.Range
                         && Storings.Player.GetSpellDamage(m, SpellSlot.E) < m.Health);
                if (minion != null)
                {
                    E.Cast(minion.Position);
                    return true;
                }
            }
            return false;
        }

        private bool KillSteal()
        {
            if (!Storings.MenuConfiguration.KillSteal.Value || 
                !Storings.MenuConfiguration.KillStealDisturbR.Value 
                && Storings.Player.HasBuff(Storings.RBUFFNAME))
            {
                return false;
            }
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                if (enemy.IsDead || !enemy.IsVisible)
                {
                    continue;
                }
                if (E.Ready && enemy.Distance(Storings.Player) < E.Range 
                    && Storings.Player.GetSpellDamage(enemy, SpellSlot.E) > enemy.Health)
                {
                    blocker.Active = false;
                    if (W.Ready)
                    {
                        W.Cast();
                        return true;
                    }
                    E.Cast(enemy.Position);
                    return true;
                }
                if (enemy.Distance(Storings.Player) < Q.Range && GetDamage(enemy, false) > enemy.Health)
                {
                    if (Q.Ready || E.Ready)
                    {
                        blocker.Active = false;
                    }
                    return SpellLogic.ESpell(enemy, true) || SpellLogic.WSpell() || SpellLogic.QSpell(true);
                }
            }
            return false;
        }

        private double GetDamage(Obj_AI_Hero target, bool includeR)
        {
            if (target == null)
            {
                return 0;
            }
            double toReturn = 0;
            if (E.Ready && Storings.DaggerManager.AllDaggers.Any(
                d => d.Distance(target) < Storings.DAGGERDAMAGERANGE + Storings.DAGGERTRIGGERRANGE))
            {
                toReturn += CalcPassiveDamage(target);
            }
            //Second check for flying Q
            if (Q.Ready || target == qTarget)
            {
                toReturn += Storings.Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (E.Ready)
            {
                toReturn += Storings.Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (R.Ready && includeR)
            {
                toReturn += Storings.Player.GetSpellDamage(target, SpellSlot.R);
            }
            return toReturn;
        }

        private double CalcPassiveDamage(Obj_AI_Hero target)
        {
            //For URF with MaxLevel > 18
            double rawDmg = Storings.PassiveDamages[Math.Min(Storings.Player.Level - 1, 17)];
            rawDmg += Storings.Player.TotalAttackDamage - Storings.Player.BaseAttackDamage;
            rawDmg += DamageMultiplier() * Storings.Player.TotalAbilityDamage;
            return Storings.Player.CalculateDamage(target, DamageType.Magical, rawDmg);
        }

        private double DamageMultiplier()
        {
            // Loss of fraction intended, so floor is not needed
            // ReSharper disable once PossibleLossOfFraction
            return (Storings.Player.Level - 1) / 5 * 0.15d + 0.55d;
        }

        private void Draw()
        {
            if (Storings.MenuConfiguration.DrawQ.Value && Q.Ready)
            {
                Render.Circle(Storings.Player.Position, Q.Range, 90, Color.Red);
            }
            if (Storings.MenuConfiguration.DrawE.Value && E.Ready)
            {
                Render.Circle(Storings.Player.Position, E.Range, 90, Color.Red);
            }
            if (Storings.MenuConfiguration.DrawR.Value && R.Ready)
            {
                Render.Circle(Storings.Player.Position, R.Range, 90, Color.Red);
            }
        }
    }
}