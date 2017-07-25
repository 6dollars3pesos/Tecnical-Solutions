using System.Collections.Generic;
using System.Linq;
using Aimtec;
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
            switch (MenuConfiguration.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    ComboMode(Selector.GetTarget(1200));
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
                && (Orbwalker.GetOrbwalkingTarget() is Obj_AI_Hero || Orbwalker.GetOrbwalkingTarget().Name == Storings.BARRELNAME))
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
                        if (barrelManager.GetBarrelsInRange(barrel).Any(b => GameObjects.EnemyHeroes.Any(
                            e => b.BarrelObject.Distance(e.Position) < 1100 && e.Distance(Player.Position) < 1000)))
                        {
                            Q.Cast(barrel.BarrelObject);
                        }
                    }
                }
            }
            
            //Extend Logic
            //Todo Verify Cooldown
            if (MenuConfiguration.ComboEExtend.Value && E.Ready && Q.GetSpell().Cooldown < 500)
            {
                foreach (var barrel in barrelManager.GetBarrelsInRange(Q.Range))
                {
                    var predictionCircle = barrelPrediction.GetPredictionCircle(
                        target, Storings.CHAINTIME + Helper.GetQTime(barrel.BarrelObject.Position));
                    var castPos = target.Position.ReduceToMaxDistance(predictionCircle.Item1, Storings.CONNECTRANGE);
                    if (!barrel.CanQNow(200) || !(castPos.Distance(target.Position) < predictionCircle.Item2))
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
    }
}