﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<Tuple<Barrel, int>> barrelsWithExplosionTimes = new List<Tuple<Barrel, int>>();
        private readonly List<Obj_AI_Hero> notHitEnemies = GameObjects.EnemyHeroes.ToList();
        private readonly BarrelPrediction bPrediction;
        private readonly int firstExplosionTime;
        
        
        public ExplosionTrigger(IEnumerable<Tuple<Barrel, int>> barrelsWithExplosionTime, BarrelPrediction bPrediction,
            bool triggeredByQ = true)
        {
            
            AttackableUnit.OnDamage += TriggerNextExplosion;

            this.bPrediction = bPrediction;
                        
            var withTime = barrelsWithExplosionTime as Tuple<Barrel, int>[] ?? barrelsWithExplosionTime.ToArray();

            firstExplosionTime = triggeredByQ
                ? Helper.GetQTime(withTime.First().Item1.BarrelObject.Position)
                : (int) (1000 * Storings.Player.AttackDelay);
            
            List<Barrel> currentBarrels = new List<Barrel>();
            
            int currentMultiplier = 0;
            int cind = 0;
            
            //Assuming Tuple to be ordered
            while (cind < withTime.Length)
            {
                while (cind < withTime.Length && currentMultiplier == withTime[cind].Item2)
                {
                    currentBarrels.Add(withTime[cind].Item1);
                    cind++;
                }
                var barrelCopy = currentBarrels.ToList();
                int delay = firstExplosionTime + currentMultiplier * Storings.CHAINTIME - Storings.EXECUTION_OFFSET;
                
                Console.WriteLine("Delayed by {0} ms", delay);
                if (delay > 0)
                {
                    if (currentMultiplier == 0 
                        && Storings.CHAMPIONIMPL.MenuConfiguration.ComboDoubleE.Value)
                    {
                        DelayAction.Queue(delay, () => TriggerAction(barrelCopy));
                    }
                    else if (currentMultiplier != 0
                             && Storings.CHAMPIONIMPL.MenuConfiguration.ComboTripleE.Value)
                    {
                        DelayAction.Queue(delay - 50, () => TriggerAction(barrelCopy));
                    }
                }
                currentBarrels.Clear();
                currentMultiplier++;
            }
            //foreach (Tuple<Barrel,int> tuple in withTime)
            //{
            //    //Trigger an Event for all Elements that get destroyed by the same time
            //    if (currentMultiplier != tuple.Item2)
            //    {
            //        var barrelCopy = currentBarrels;
            //        Console.WriteLine("Delayed by {0} ms", firstExplosionTime + tuple.Item2 * Storings.CHAINTIME - Storings.EXECUTION_OFFSET);
            //        DelayAction.Queue(
            //            firstExplosionTime + tuple.Item2 * Storings.CHAINTIME - Storings.EXECUTION_OFFSET,
            //            () => TriggerAction(barrelCopy));
            //        currentBarrels.Clear();
            //        currentMultiplier = tuple.Item2;
            //    }
            //    currentBarrels.Add(tuple.Item1);
            //    barrelsWithExplosionTimes.Add(
            //        new Tuple<Barrel, int>(tuple.Item1, firstExplosionTime + tuple.Item2 * Storings.CHAINTIME));
            //}
            //DelayAction.Queue(
            //    firstExplosionTime + withTime.Last().Item2 * Storings.CHAINTIME - Storings.EXECUTION_OFFSET,
            //    () => TriggerAction(currentBarrels));
            //Console.WriteLine("Delayed by {0} ms", firstExplosionTime + withTime.Last().Item2 * Storings.CHAINTIME - Storings.EXECUTION_OFFSET);
            //firstExplosionTime += Game.TickCount;
        }

        private void TriggerNextExplosion(AttackableUnit attackableUnit, AttackableUnitDamageEventArgs eventArgs)
        {
            if (attackableUnit.Name != Storings.BARRELNAME)
            {
                return;
            }
            if (barrelsWithExplosionTimes.RemoveAll(t => t.Item1.BarrelObject == attackableUnit) != 0)
            {
                if (eventArgs.Source.IsMe)
                {
//                    for (int i = notHitEnemies.Count - 1; i >= 0; i--)
//                    {
//                        if (notHitEnemies[i].Distance(attackableUnit) < Storings.BARRELRANGE)
//                        {
//                            notHitEnemies.RemoveAt(i);
//                        }
//                    }
                }
            }
            if (!barrelsWithExplosionTimes.Any())
            {
                AttackableUnit.OnDamage -= TriggerNextExplosion;
            }
        }

        private int GetPredictionDelay(int bounces)
        {
            return Game.TickCount + bounces * Storings.CHAINTIME - firstExplosionTime;
        }

        private void TriggerAction(List<Barrel> extendableBarrels)
        {
            Spell e = Storings.CHAMPIONIMPL.E;
            if (!e.Ready)
            {
                return;
            }
            ReduceRemainingEnemies();
            Console.WriteLine("{0} Enemies remaining", notHitEnemies.Count);
            if (!notHitEnemies.Any())
            {
                return;
            }
            //Calculating here the optimal Cast position
            ITargetSelector selector = TargetSelector.Implementation;
            var orderedTargets = selector.GetOrderedTargets(e.Range + Storings.BARRELRANGE);
            foreach (var target in orderedTargets)
            {
                Console.WriteLine("Target there");
                if (!notHitEnemies.Contains(target) 
                    || !extendableBarrels.Any(b => b.BarrelObject.Distance(target) < Storings.BARRELRANGE * 3))
                {
                    Console.WriteLine("{0} Barrels there", extendableBarrels.Count);
                    continue;
                }
                Console.WriteLine("Trying to hit Target");
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
                    Storings.CONNECTRANGE - 10, Storings.Player.Position.To2D(), e.Range);
                List<Vector3> optimalPositions = new List<Vector3>(4);
                foreach (Vector2 vector in intersections)
                {
                    optimalPositions.Add(vector.To3D());
                }
                optimalPositions.Add(Storings.Player.Position.ReduceToMaxDistance(extCircle.Item1, e.Range));
                optimalPositions.Add(barrelPosition.ReduceToMaxDistance(extCircle.Item1, Storings.CONNECTRANGE - 10));
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
                    return;
                }
            }
            //Todo Nice Algorithm that hits multiple enemies

//            Vector3[] barrelPositions = new Vector3[extendableBarrels.Count];
//            for (int i = 0; i < barrelPositions.Length; i++)
//            {
//                barrelPositions[i] = extendableBarrels[i].BarrelObject.Position;
//            }
//            time = Environment.TickCount - time;
            
        }

        private void ReduceRemainingEnemies()
        {
            notHitEnemies.RemoveAll(e =>
                barrelsWithExplosionTimes.Any(barrelTimeTuple => bPrediction.CannotEscape(barrelTimeTuple.Item1,
                    e, GetPredictionDelay(barrelTimeTuple.Item2))));
        }
    }
}