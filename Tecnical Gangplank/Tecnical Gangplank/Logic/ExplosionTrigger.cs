﻿using System;
using System.Collections.Generic;
using System.Linq;
 using System.Media;
 using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.TargetSelector;
using Aimtec.SDK.Util;
using Aimtec.SDK.Util.Cache;
using TecnicalGangplank.Configurations;
using TecnicalGangplank.Extensions;
using TecnicalGangplank.Prediction;
using Spell = Aimtec.SDK.Spell;

namespace TecnicalGangplank.Logic
{
    public class ExplosionTrigger
    {
        private readonly List<Tuple<Barrel, int>> barrelsWithExplosionTimes;
        private readonly List<Obj_AI_Hero> notHitEnemies = GameObjects.EnemyHeroes.ToList();
        private readonly BarrelPrediction bPrediction;
        private readonly int firstExplosionTime;
        
        
        public ExplosionTrigger(IEnumerable<Tuple<Barrel, int>> barrelsWithExplosionTime, BarrelPrediction bPrediction,
            bool triggeredByQ = true)
        {
            barrelsWithExplosionTimes = barrelsWithExplosionTime.ToList();
            AttackableUnit.OnDamage += TriggerNextExplosion;

            this.bPrediction = bPrediction;
                        
            firstExplosionTime = triggeredByQ
                ? Helper.GetQTime(barrelsWithExplosionTimes.First().Item1.BarrelObject.Position)
                : 0;
            
            List<Barrel> currentBarrels = new List<Barrel>();
            
            int currentMultiplier = 0;
            int cind = 0;
            
            //Assuming Tuple to be ordered
            while (cind < barrelsWithExplosionTimes.Count)
            {
                while (cind < barrelsWithExplosionTimes.Count && currentMultiplier == barrelsWithExplosionTimes[cind].Item2)
                {
                    currentBarrels.Add(barrelsWithExplosionTimes[cind].Item1);
                    cind++;
                }
                var barrelCopy = currentBarrels.ToList();
                int delay = firstExplosionTime + currentMultiplier * Storings.CHAINTIME - Storings.EXECUTION_OFFSET - Game.Ping/2;
                
                if (delay > 0)
                {
                    if (currentMultiplier == 0 
                        && Storings.Player.Distance(currentBarrels[0].BarrelObject) > Storings.ChampionImpl.Q.Range - 25
                        && Storings.MenuConfiguration.ComboDoubleE.Value)
                    {
                        // ReSharper disable once ObjectCreationAsStatement
                        new AsapAction<List<Barrel>>(700, TriggerAction, barrelCopy);
                    }
                    else if (currentMultiplier != 0
                             && Storings.MenuConfiguration.ComboTripleE.Value)
                    {
                        DelayAction.Queue(delay, () => TriggerAction(barrelCopy));
                    }
                }
                currentBarrels.Clear();
                currentMultiplier++;
            }
            firstExplosionTime += Game.TickCount;
        }

        private void TriggerNextExplosion(AttackableUnit attackableUnit, AttackableUnitDamageEventArgs eventArgs)
        {
            if (attackableUnit.Name != Storings.BARRELNAME)
            {
                return;
            }
            barrelsWithExplosionTimes.RemoveAll(t => t.Item1.BarrelObject == attackableUnit) ;
            if (!barrelsWithExplosionTimes.Any())
            {
                AttackableUnit.OnDamage -= TriggerNextExplosion;
            }
        }

        private int GetPredictionDelay(int bounces)
        {
            return Game.TickCount + bounces * Storings.CHAINTIME - firstExplosionTime;
        }

        private bool TriggerAction(List<Barrel> extendableBarrels)
        {
            Spell e = Storings.ChampionImpl.E;
            if (!e.Ready)
            {
                return false;
            }
            ReduceRemainingEnemies();
            if (!notHitEnemies.Any())
            {
                return true;
            }
            //Calculating here the optimal Cast position
            ITargetSelector selector = TargetSelector.Implementation;
            var orderedTargets = selector.GetOrderedTargets(e.Range + Storings.BARRELRANGE);
            foreach (var target in orderedTargets)
            {
                if (!notHitEnemies.Contains(target) 
                    || !extendableBarrels.Any(b => b.BarrelObject.Distance(target) < Storings.BARRELRANGE * 3))
                {
                    continue;
                }
                float dist = float.MaxValue;
                Vector3 barrelPosition = Vector3.Zero;
                foreach (Barrel barrel in extendableBarrels)
                {
                    float dist2 = barrel.BarrelObject.Distance(target);
                    if (dist2 < dist)
                    {
                        barrelPosition = barrel.BarrelObject.Position;
                        dist = dist2;
                    }
                }
                Tuple<Vector3, float> extCircle =
                    bPrediction.GetPredictionCircle(target, Storings.EXECUTION_OFFSET);
                Vector2[] intersections = Helper.IntersectCircles(barrelPosition.To2D(),
                    Storings.CONNECTRANGE - 10, Storings.Player.Position.To2D(), e.Range - 10);
                List<Vector3> optimalPositions = new List<Vector3>(4);
                foreach (Vector2 vector in intersections)
                {
                    optimalPositions.Add(vector.To3D());
                }
                optimalPositions.Add(Storings.Player.Position.ReduceToMaxDistance(extCircle.Item1, e.Range));
                Vector3 tempPos = barrelPosition.ReduceToMaxDistance(extCircle.Item1, Storings.CONNECTRANGE - 10);
                if (tempPos.Distance(Storings.Player) <= e.Range)
                {
                    optimalPositions.Add(tempPos);
                }
                dist = float.MaxValue;
                foreach (var position in optimalPositions)
                {
                    float dist2 = position.Distance(extCircle.Item1);
                    if (dist2 < dist && dist2 < extCircle.Item2 && barrelPosition.Distance(position) <= Storings.CONNECTRANGE)
                    {
                        barrelPosition = position;
                        dist = dist2;
                    }
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (dist != float.MaxValue)
                {
                    e.Cast(barrelPosition);
                    return true;
                }
            }
            //Todo Nice Algorithm that hits multiple enemies

//            Vector3[] barrelPositions = new Vector3[extendableBarrels.Count];
//            for (int i = 0; i < barrelPositions.Length; i++)
//            {
//                barrelPositions[i] = extendableBarrels[i].BarrelObject.Position;
//            }
//            time = Environment.TickCount - time;
            return true;
        }

        private void ReduceRemainingEnemies()
        {
            notHitEnemies.RemoveAll(e =>
                barrelsWithExplosionTimes.Any(barrelTimeTuple => bPrediction.CannotEscape(barrelTimeTuple.Item1,
                    e, GetPredictionDelay(barrelTimeTuple.Item2))));
        }
        
        private class AsapAction<T>
        {
            private readonly int expireTime;
            private readonly Func<T, bool> customAction;
            private readonly T value;

            public AsapAction(int expireTicks, Func<T, bool> customAction, T value)
            {
                expireTime = expireTicks + Game.TickCount;
                this.customAction = customAction;
                this.value = value;
                Game.OnUpdate += ActionWrapper;
            }

            private void ActionWrapper()
            {
                if (Game.TickCount > expireTime || customAction(value))
                {
                    Game.OnUpdate -= ActionWrapper;
                }
            }
        }
    }
}