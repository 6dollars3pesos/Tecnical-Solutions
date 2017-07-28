﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.TargetSelector;
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
        private IOrbwalker Orbwalker => MenuConfiguration.Orbwalker;

        public Gangplank() : this(new []{625, 0, 1000, float.MaxValue})
        {
        }
        
        private Gangplank(float[] ranges) : base(ranges)
        {
            barrelPrediction = new BarrelPrediction(barrelManager);
        }
        
        
        

        public override void UpdateGame()
        {
            if (!E.Ready)
            {
                Console.WriteLine("Cooldown of E is {}", E.GetSpell().Cooldown);
            }
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
                    //Todo explicit Laneclear Mode
                    LastHitMode();
                    break;
            }
        }



        public override void LoadGame()
        {
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
            Barrel attackable = null;
            if (MenuConfiguration.ComboAABarrel.Value)
            {
                IEnumerable<Barrel> barrels = barrelManager.
                    GetBarrelsInRange(Player.AttackRange).
                    Where(b => b.CanAANow());
                
                //Noone in Range to Attack
                var enumerable = barrels as Barrel[] ?? barrels.ToArray();
                if (enumerable.Any() && Orbwalker.GetOrbwalkingTarget() == null && Orbwalker.CanAttack())
                {
                    attackable =
                        enumerable.FirstOrDefault(b => barrelPrediction.CanHitEnemy(b, target, Player.AttackDelay));
                    if (attackable != null)
                    {
                        Orbwalker.ForceTarget(attackable.BarrelObject);
                    }
                }
            }
            if (attackable == null)
            {
                Orbwalker.ForceTarget(null);
            }
            if (MenuConfiguration.ComboQBarrel.Value && Q.Ready)
            {
                //Direct Q to Barrel
                foreach (Barrel barrel in barrelManager.GetBarrelsInRange(Q.Range))
                {
                    if (!barrel.CanQNow() ||
                        !barrelPrediction.CanHitEnemy(barrel, target, Helper.GetQTime(barrel.BarrelObject.Position)))
                    {
                        continue;
                    }
                    Console.WriteLine("Casting Q from here");
                    Q.Cast(barrel.BarrelObject);
                    return;
                }
                
                //Double-Logic
                if (MenuConfiguration.ComboDoubleE.Value 
                    //Todo Verify Cooldown
                    && (E.Ready || E.GetSpell().Cooldown < 480))
                {
                    foreach (Barrel barrel in barrelManager.GetBarrelsInRange(Q.Range + Storings.QDELTA))
                    {
                        if (!barrel.CanQNow() || !(barrel.BarrelObject.Distance(target) < 850))
                        {
                            continue;
                        }
                        Q.Cast(barrel.BarrelObject);
                        return;
                    }
                }
                
                //Triple-Logic
                if (MenuConfiguration.ComboTripleE.Value
                    //Todo Verfiy Cooldown
                    && (E.Ready || E.GetSpell().Cooldown < 
                        Storings.CHAINTIME + Storings.QDELAY - Storings.EXECUTION_OFFSET))
                {
                    foreach (var barrel in barrelManager.GetBarrelsInRange(Q.Range))
                    {
                        if (barrel.CanQNow() 
                            && barrelManager.GetBarrelsInRange(barrel).Any(b => GameObjects.EnemyHeroes.Any(
                            e => b.BarrelObject.Distance(e.Position) < 1100 && e.Distance(Player.Position) < 1000)))
                        {
                            Q.Cast(barrel.BarrelObject);
                        }
                    }
                }
            }
            
            //Extend Logic
            //Todo Verify Cooldown
            if (MenuConfiguration.ComboEExtend.Value && E.Ready && Q.GetSpell().Cooldown < 500
                && !barrelManager.GetBarrelsInRange(target.Position, Storings.BARRELRANGE - 100).Any())
            {
                foreach (var barrel in barrelManager.GetBarrelsInRange(Q.Range))
                {
                    if (!barrel.CanQNow(200))
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
            if (MenuConfiguration.ComboQ.Value && Q.Ready && !E.Ready 
                && target.Distance(Player) < Q.Range && !barrelManager.GetBarrelsInRange(Q.Range + 200).Any())
            {
                Q.Cast(target);
            }

            if (E.Ready && MenuConfiguration.ComboE.Value && E.GetSpell().Ammo > 2
                && !barrelManager.GetBarrelsInRange(Q.Range + 200).Any())
            {
                Vector3 castPosition = barrelPrediction.GetPredictedPosition(target);
                if (castPosition.Distance(Player.Position) < E.Range)
                {
                    E.Cast(castPosition);
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
                //Todo get Minion with least Health
                
                //Lasthitting with Q to Minion
                else if (MenuConfiguration.LastHitQ.Value)
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
            //Todo Killsteal with Barrel (depending on Performance)
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
            //Todo verify Cooldown
            if (MenuConfiguration.KeyDetonation.Value && MenuConfiguration.KeyDetonationKey.Value && E.Ready
                && Q.GetSpell().Cooldown < 1000)
            {
                Barrel nearestBarrel = barrelManager.GetNearestBarrel(Game.CursorPos);
                if (nearestBarrel != null)
                {
                    Vector3 castPos =
                        nearestBarrel.BarrelObject.Position.ReduceToMaxDistance(Game.CursorPos, Storings.CONNECTRANGE);
                    E.Cast(castPos);
                    // ReSharper disable once ObjectCreationAsStatement
                    new SpellQueuer(Q, nearestBarrel.BarrelObject, 3000);
                }
            }
        }
    }
}