using System;
using System.Collections;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.TargetSelector;
using Aimtec.SDK.Util;
using Aimtec.SDK.Util.Cache;
using TecnicalGangplank.Configurations;
using TecnicalGangplank.Extensions;
using TecnicalGangplank.Prediction;

namespace TecnicalGangplank.Logic
{
    internal class Gangplank : Champion
    {
        private readonly BarrelManager barrelManager = new BarrelManager();
        private readonly BarrelPrediction barrelPrediction;

        private Obj_AI_Hero Player => Storings.Player;
        private ITargetSelector Selector => Storings.Selector;
        private readonly Config MenuConfiguration = Storings.MenuConfiguration;
        private IOrbwalker Orbwalker => MenuConfiguration.Orbwalker;
        private readonly TargetGetter targetGetter = new TargetGetter(1200);
        private bool correctedCast;
        private CancellationTokenSource canceller;

        public Gangplank() : this(new []{625, 0, 1000, float.MaxValue})
        {
        }
        
        private Gangplank(float[] ranges) : base(ranges)
        {
            barrelPrediction = new BarrelPrediction(barrelManager);
        }
        
        
        

        public override void UpdateGame()
        {
            Keys();
            Cleanse();
            Killsteal();
            switch (MenuConfiguration.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    ComboMode(Selector.GetTarget(1200));
                    break;
                case OrbwalkingMode.Lasthit:
                    LastHitMode();
                    break;
                case OrbwalkingMode.Laneclear:
                    LaneClearMode();
                    break;
            }
        }



        public override void LoadGame()
        {
            Render.OnPresent += Draw;
            SpellBook.OnCastSpell += OnCastSpell;
        }

        private void OnCastSpell(Obj_AI_Base sender, SpellBookCastSpellEventArgs e)
        {
            if (sender.IsMe && e.Slot == SpellSlot.E)
            {
                PreventCast(e);
            }
        }



        protected override void ProcessPlayerCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            switch (e.SpellSlot)
            {
                case SpellSlot.E:
                    //Action for E
                    return;
                case SpellSlot.Q:
                    if ((MenuConfiguration.ComboTripleE.Value || 
                         MenuConfiguration.MiscExtendE.Value || 
                         MenuConfiguration.ComboDoubleE.Value) 
                        && e.Target.Name == Storings.BARRELNAME)
                    {
                        // ReSharper disable once ObjectCreationAsStatement
                        new ExplosionTrigger(barrelManager.GetBarrelsWithBounces((Obj_AI_Minion)e.Target), barrelPrediction);
                    }
                    //Action for Q
                    return;
            }
        }


        protected override void ProcessPlayerAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            if (MenuConfiguration.MiscExtendE.Value && e.Target.Name == Storings.BARRELNAME)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new ExplosionTrigger(
                    barrelManager.GetBarrelsWithBounces((Obj_AI_Minion) e.Target), barrelPrediction, false);
            }
        }

        /// <summary>
        /// Combo - Core of the Assembly
        /// <para>Codeflow:</para>
        /// <para>Autoattack Barrel</para>
        /// <para>Use Q on Barrel</para>
        /// <para>Use Q for Double E on Barrel</para>
        /// <para>Use Q for Triple E on Barrel</para>
        /// <para>Use E for Triple E to extend existing Barrel</para>
        /// <para>Use E to Extend existing Barrel</para>
        /// <para>Use E to Place new Barrel (only on 3 Barrels)</para>
        /// </summary>
        /// <param name="target">Target</param>
        private void ComboMode(Obj_AI_Hero target)
        {
            if (target == null 
                || Orbwalker.IsWindingUp 
                && (Orbwalker.GetOrbwalkingTarget() is Obj_AI_Hero ||
                    Orbwalker.GetOrbwalkingTarget().Name == Storings.BARRELNAME))
            {
                return;
            }
            target = targetGetter.getTarget(800);
            if (MenuConfiguration.ComboAABarrel.Value && target != null && !Player.HasBuffOfType(BuffType.Blind))
            {
                
                IEnumerable<Barrel> barrels = barrelManager.
                    GetBarrelsInRange(Storings.BARRELAARANGE).
                    Where(b => b.CanAANow());
                //Noone in Range to Attack
                var enumerable = barrels as Barrel[] ?? barrels.ToArray();
                if (enumerable.Any() && Orbwalker.GetOrbwalkingTarget() == null && Orbwalker.CanAttack())
                {
                    var attackable = enumerable.FirstOrDefault(b => barrelPrediction.CanHitEnemy(b, target, Player.AttackDelay));
                    if (attackable != null)
                    {
                        Orbwalker.ForceTarget(attackable.BarrelObject);
                        return;
                    }
                }
            }
            Orbwalker.ForceTarget(null);
            if (MenuConfiguration.ComboQBarrel.Value && Q.Ready)
            {
                //Direct Q to Barrel
                foreach (Barrel barrel in barrelManager.GetBarrelsInRange(Q.Range))
                {
                    if (!barrel.CanQNow() ||
                        !barrelPrediction.CanHitEnemy(barrel, target, Helper.GetQTime(barrel.BarrelObject.Position))
                        || barrel.BarrelObject.Distance(Player) < Storings.BARRELAARANGE 
                        && MenuConfiguration.ComboAABarrel.Value && !barrel.CanAANow())
                    {
                        continue;
                    }
                    Q.Cast(barrel.BarrelObject);
                    return;
                }
                
                //Double-Logic
                if (MenuConfiguration.ComboDoubleE.Value 
                    && (E.Ready || E.GetSpell().CooldownEnd - Game.ClockTime < 0.48))
                {
                    foreach (Barrel barrel in barrelManager.GetBarrelsInRange(Q.Range + 50))
                    {
                        if (!barrel.CanQNow() || !(barrel.BarrelObject.Distance(target) < 850) ||
                            barrel.BarrelObject.Distance(Player) < Q.Range + 30)
                        {
                            continue;
                        }
                        MenuConfiguration.Orbwalker.MovingEnabled = false;
                        canceller?.Cancel();
                        canceller = DelayAction.Queue(400, () => MenuConfiguration.Orbwalker.MovingEnabled = true);
                        Q.Cast(barrel.BarrelObject);
                        return;
                    }
                }
                
                //Triple-Logic (Q Trigger)
                target = targetGetter.getTarget(1200);
                if (target != null && MenuConfiguration.ComboTripleE.Value
                    && (E.Ready || E.GetSpell().CooldownEnd - Game.ClockTime < 
                        0.001f * Storings.CHAINTIME + Storings.QDELAY - Storings.EXECUTION_OFFSET))
                {
                    foreach (var barrel in barrelManager.GetBarrelsInRange(Q.Range))
                    {
                        if (barrel.CanQNow() 
                            && barrelManager.GetBarrelsInRange(barrel).Any(b => 
                                b.BarrelObject.Distance(target.Position) < Storings.BARRELRANGE * 2.5))
                        {
                            Q.Cast(barrel.BarrelObject);
                            return;
                        }
                    }
                }
                //Triple-Logic (E Trigger)
                if (MenuConfiguration.ComboTripleE.Value && E.Ready && E.GetSpell().Ammo > 1
                    && (Q.Ready || Q.GetSpell().CooldownEnd - Game.ClockTime < 0.65f))
                {
                    Vector3 predictedPosition = barrelPrediction.GetPredictedPosition(target);
                    Barrel suitableBarrel =
                        barrelManager.GetNearestBarrel(predictedPosition);
                    if (suitableBarrel != null 
                        && suitableBarrel.BarrelObject.Distance(predictedPosition) > Storings.BARRELRANGE * 2.5
                        && suitableBarrel.BarrelObject.Distance(predictedPosition) < Storings.BARRELRANGE * 4
                        && suitableBarrel.CanQNow(650)
                        && target.Distance(Player) < E.Range)
                    {
                        E.Cast(suitableBarrel.BarrelObject.Position.ReduceToMaxDistance(predictedPosition,
                            Storings.CONNECTRANGE));
                        return;
                    }
                }
            }
            
            //Extend Logic
            target = targetGetter.getTarget(1000);
            if (target != null && MenuConfiguration.ComboEExtend.Value && E.Ready 
                && !barrelManager.GetBarrelsInRange(target.Position, Storings.BARRELRANGE - 100).Any())
            {
                bool autoAttackNeeded = Q.Ready || Q.GetSpell().CooldownEnd - Game.ClockTime < 0.8f;
                float barrelSearchRange = autoAttackNeeded ? Q.Range : Storings.BARRELAARANGE;
                foreach (var barrel in barrelManager.GetBarrelsInRange(barrelSearchRange))
                {
                    if (barrel.BarrelObject.Distance(target) < Storings.BARRELRANGE || 
                        !autoAttackNeeded && !barrel.CanQNow(650) || autoAttackNeeded && !barrel.CanAANow(650))
                    {
                        continue;
                    }
                    var predictionCircle = barrelPrediction.GetPredictionCircle(
                        target, Storings.CHAINTIME + Helper.GetQTime(barrel.BarrelObject.Position));
                    var castPos = barrel.BarrelObject.Position.ReduceToMaxDistance(predictionCircle.Item1, Storings.CONNECTRANGE);
                    if (!(castPos.Distance(target.Position) < predictionCircle.Item2))
                    {
                        continue;
                    }
                    E.Cast(castPos);
                    return;
                }
            }

            //Q Cast on Enemy
            target = targetGetter.getTarget((int)Q.Range);
            if (target != null && MenuConfiguration.ComboQ.Value && Q.Ready 
                && (!E.Ready && E.GetSpell().CooldownEnd - Game.ClockTime > 1f || 
                    !barrelManager.GetBarrelsInRange(Q.Range + 200).Any(b => b.BarrelObject.Distance(target) < 
                                                                             Storings.BARRELRANGE * 2.5)) 
                && target.Distance(Player) < Q.Range)
            {
                Q.Cast(target);
                return;
            }

            target = targetGetter.getTarget(800);
            if (target != null && E.Ready && E.GetSpell().Ammo >= MenuConfiguration.ComboEMinimum.Value
                && !barrelManager.GetBarrelsInRange(Q.Range + 200).Any(b => b.BarrelObject.Distance(target) < 
                                                                            Storings.BARRELRANGE * 2.5))
            {
                Vector3 castPosition = barrelPrediction.GetPredictedPosition(target);
                if (castPosition.Distance(Player.Position) < E.Range)
                {
                    E.Cast(castPosition);
                    // ReSharper disable once RedundantJumpStatement
                    return;
                }
            }
        }
        
        
        /// <summary>
        /// Lasthit Mode
        /// <para>Codeflow:</para>
        /// <para>Q on Barrel</para>
        /// <para>Q on Minion</para>
        /// </summary>
        private void LastHitMode()
        {
            if (Q.Ready && (MenuConfiguration.LastHitBarrelQ.Value || MenuConfiguration.LastHitQ.Value))
            {
                IEnumerable<Barrel> barrels = barrelManager.GetBarrelsInRange(Q.Range);
                var enumerable = barrels as Barrel[] ?? barrels.ToArray();
                
                //Lasthitting with Q to Barrel
                if (MenuConfiguration.LastHitBarrelQ.Value && enumerable.Any())
                {
                    var attackableBarrel = enumerable.FirstOrDefault(
                        b => b.CanQNow() &&
                        GameObjects.EnemyMinions.Count(
                                 m => m.Health <= Player.GetSpellDamage(m, SpellSlot.Q) &&
                                     m.Distance(b.BarrelObject) < Storings.BARRELRANGE) >=
                             MenuConfiguration.LastHitMinimumQ.Value);
                    if (attackableBarrel != null)
                    {
                        Q.Cast(attackableBarrel.BarrelObject);
                    }
                }
                //Todo Health Prediction                
                //Lasthitting with Q to Minion
                else if (MenuConfiguration.LastHitQ.Value)
                {
                    var attackingMinion = GameObjects.EnemyMinions.Where(e => e.Distance(Player) <= Q.Range && e.Health < Player.GetSpellDamage(e, SpellSlot.Q))
                        .MinBy(m => m.Health);
                    if (attackingMinion != null)
                    {
                        Q.Cast(attackingMinion);
                    }
                }
            }
        }
        
        /// <summary>
        /// Laneclear Mode
        /// <para>Codeflow:</para>
        /// <para>Q on Barrel</para>
        /// <para>Q on Minion</para>
        /// </summary>
        private void LaneClearMode()
        {
            if (Q.Ready && (MenuConfiguration.LaneClearBarrelQ.Value || MenuConfiguration.LaneClearQ.Value))
            {
                IEnumerable<Barrel> barrels = barrelManager.GetBarrelsInRange(Q.Range);
                var enumerable = barrels as Barrel[] ?? barrels.ToArray();
                
                //Laneclearing with Q to Barrel
                if (MenuConfiguration.LaneClearBarrelQ.Value && enumerable.Any())
                {
                    var attackableBarrel = enumerable.FirstOrDefault(
                        b => b.CanQNow() &&
                             GameObjects.EnemyMinions.Count(
                                 m => m.Health <= Player.GetSpellDamage(m, SpellSlot.Q) &&
                                      m.Distance(b.BarrelObject) < Storings.BARRELRANGE) >=
                             MenuConfiguration.LaneClearMinimumQ.Value);
                    if (attackableBarrel != null)
                    {
                        Q.Cast(attackableBarrel.BarrelObject);
                    }
                }
                
                //Laneclearing with Q to Minion
                else if (MenuConfiguration.LaneClearQ.Value)
                {
                    var attackingMinion = GameObjects.EnemyMinions.FirstOrDefault
                        (e => e.Distance(Player) <= Q.Range && e.Health < Player.GetSpellDamage(e, SpellSlot.Q));
                    if (attackingMinion != null)
                    {
                        Q.Cast(attackingMinion);
                    }
                }
            }
        }

        
        private void Killsteal()
        {
            if (MenuConfiguration.KillStealQ.Value)
            {
                Obj_AI_Hero target = GameObjects.EnemyHeroes.FirstOrDefault(
                    e => e.Distance(Player) < Q.Range && e.Health < Player.GetSpellDamage(e, SpellSlot.Q));
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            //Todo More enemies
            if (MenuConfiguration.KillStealR.Value)
            {
                Obj_AI_Hero target = Selector.GetTarget(10000);
                int wavecount = (int) Math.Ceiling(target.Health / Player.GetSpellDamage(target, SpellSlot.R));
                if (wavecount <= 3)
                {
                    Vector3 castPos = barrelPrediction.GetPredictedPosition(target);
                    // ReSharper disable once AccessToModifiedClosure
                    var addTarget = GameObjects.EnemyHeroes.Where(e => e != target).MinBy(e => e.Distance(castPos));
                    if (addTarget.Distance(castPos) < 700)
                    {
                        castPos = castPos.ReduceToMaxDistance(addTarget.Position, 200);
                    }
                    R.Cast(castPos);
                    return;
                }
                if (wavecount <= 6)
                {
                    R.Cast(barrelPrediction.GetPredictedPosition(target));
                    return;
                }
                if (wavecount <= 9)
                {
                    if (GameObjects.AllyHeroes.Any(a => a.Distance(target) < 200))
                    {
                        R.Cast(barrelPrediction.GetPredictedPosition(target));
                    }
                }
            }
        }

        /// <summary>
        /// Uses W if the player has a cleansable Debuff
        /// </summary>
        private void Cleanse()
        {
            if (W.Ready && MenuConfiguration.EnabledBuffs.Any(b => b.Value.Enabled && Player.HasBuffOfType(b.Key)))
            {
                W.Cast();
            }
        }
        
        /// <summary>
        /// All custom Keys
        /// </summary>
        private void Keys()
        {
            if (MenuConfiguration.KeyDoDetonation.Value && MenuConfiguration.KeyDetonationKey.Value)
            {
                if (MenuConfiguration.KeyDetonationOrbwalk.Value)
                {
                    Orbwalker.Move(Game.CursorPos);
                }
                if (E.Ready && (Q.Ready || Q.GetSpell().CooldownEnd - Game.ClockTime < 1))
                {
                    Barrel nearestBarrel = barrelManager.GetNearestBarrel(Game.CursorPos);
                    if (nearestBarrel != null && nearestBarrel.CanQNow(100)
                        && nearestBarrel.BarrelObject.Distance(Player) < Q.Range
                        && !barrelManager.GetBarrelsInRange(nearestBarrel, Storings.BARRELRANGE).Any())
                    {
                        Vector3 castPos =
                            nearestBarrel.BarrelObject.Position.ReduceToMaxDistance(Game.CursorPos,
                                Storings.CONNECTRANGE);
                        E.Cast(castPos);
                        // ReSharper disable once ObjectCreationAsStatement
                        new SpellQueuer(Q, nearestBarrel.BarrelObject, 3000);
                    }
                }
            }

            if (MenuConfiguration.KeyDoExplodeNextBarrel.Value 
                && MenuConfiguration.KeyExplodeNextBarrelKey.Value && Q.Ready)
            {
                Barrel nearestBarrel = barrelManager.GetNearestBarrel(Player.Position);
                if (nearestBarrel.BarrelObject.Distance(Player) < Q.Range && nearestBarrel.CanQNow())
                {
                    Q.Cast(nearestBarrel.BarrelObject);
                }
            }
        }

        private void Draw()
        {
            if (MenuConfiguration.DrawQ.Value && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 90, Color.Red);
            }
            if (MenuConfiguration.DrawE.Value && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 90, Color.Red);
            }
            if (MenuConfiguration.DrawConnectionRange.Value)
            {
                foreach (var barrel in barrelManager.GetBarrels())
                {
                    Render.Circle(barrel.BarrelObject.Position, Storings.CONNECTRANGE, 90, Color.Green);
                }
            }
        }
        
        private void PreventCast(SpellBookCastSpellEventArgs eventArgs)
        {
            int correctValue = MenuConfiguration.MiscChainCorrection.Value;
            if (correctValue == 0 || correctedCast)
            {
                correctedCast = false;
                return;
            }
            Barrel nearestBarrel = barrelManager.GetNearestBarrel(eventArgs.End);
            if (nearestBarrel == null)
            {
                return;
            }
            float deltaDist = nearestBarrel.BarrelObject.Distance(eventArgs.End);
            if (deltaDist > Storings.CONNECTRANGE && deltaDist < Storings.CONNECTRANGE + correctValue)
            {
                Vector3 castPos = nearestBarrel.BarrelObject.Position.Extend(eventArgs.End, Storings.CONNECTRANGE);
                if (!NavMesh.WorldToCell(castPos).Flags.HasFlag(NavCellFlags.Building | NavCellFlags.Wall))
                {
                    eventArgs.Process = false;
                    correctedCast = true;
                    E.Cast(nearestBarrel.BarrelObject.Position.Extend(eventArgs.End, Storings.CONNECTRANGE));
                }
            }
        }
    }
}
