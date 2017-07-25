using System;
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
            
            AttackableUnit.OnDamage += TriggerExplosion;

            this.bPrediction = bPrediction;
                        
            var withTime = barrelsWithExplosionTime as Tuple<Barrel, int>[] ?? barrelsWithExplosionTime.ToArray();
            
            firstExplosionTime = Game.TickCount + 
                (triggeredByQ ? Helper.GetQTime(withTime.First().Item1.BarrelObject.Position) : 
                    (int)(1000 * Storings.Player.AttackDelay));
            
            List<Barrel> currentBarrels = new List<Barrel>();
            
            int currentMultiplier = 0;
            foreach (Tuple<Barrel,int> tuple in withTime)
            {
                //Trigger an Event for all Elements that get destroyed by the same time
                if (currentMultiplier != tuple.Item2)
                {
                    var barrelCopy = currentBarrels;
                    DelayAction.Queue(
                        firstExplosionTime + tuple.Item2 * Storings.CHAINTIME - Storings.EXECUTION_OFFSET,
                        () => TriggerAction(barrelCopy));
                    currentBarrels.Clear();
                    currentMultiplier = tuple.Item2;
                }
                currentBarrels.Add(tuple.Item1);
                barrelsWithExplosionTimes.Add(
                    new Tuple<Barrel, int>(tuple.Item1, firstExplosionTime + tuple.Item2 * Storings.CHAINTIME));
            }
            DelayAction.Queue(
                firstExplosionTime + withTime.Last().Item2 * Storings.CHAINTIME - Storings.EXECUTION_OFFSET,
                () => TriggerAction(currentBarrels));

        }

        private void TriggerExplosion(AttackableUnit attackableUnit, AttackableUnitDamageEventArgs eventArgs)
        {
            if (attackableUnit.Name != Storings.BARRELNAME)
            {
                return;
            }
            if (barrelsWithExplosionTimes.RemoveAll(t => t.Item1.BarrelObject == attackableUnit) != 0)
            {
                if (eventArgs.Source.IsMe)
                {
                    for (int i = notHitEnemies.Count - 1; i >= 0; i--)
                    {
                        if (notHitEnemies[i].Distance(attackableUnit) < Storings.BARRELRANGE)
                        {
                            notHitEnemies.RemoveAt(i);
                        }
                    }
                }
            }
            if (!barrelsWithExplosionTimes.Any())
            {
                AttackableUnit.OnDamage -= TriggerExplosion;
            }
        }

        private int GetPredictionDelay(int bounces)
        {
            return Game.TickCount + bounces * Storings.CHAINTIME - firstExplosionTime;
        }

        private void TriggerAction(List<Barrel> extendableBarrels)
        {
            long time = Environment.TickCount;
            Spell e = Storings.CHAMPIONIMPL.E;
            if (!e.Ready)
            {
                return;
            }
            List<Obj_AI_Hero> remainingEnemies = GetRemainingEnemies();
            for (int i = remainingEnemies.Count - 1; i >= 0; i--)
            {
                if (remainingEnemies[i].Distance(Storings.Player) > e.Range + Storings.BARRELRANGE
                    || !extendableBarrels.Any(b => b.BarrelObject.Distance(Storings.Player) < Storings.BARRELRANGE * 3))
                {
                    remainingEnemies.RemoveAt(i);
                }
            }
            if (!remainingEnemies.Any())
            {
                return;
            }
            //Calculating here the optimal Cast position
            ITargetSelector selector = TargetSelector.Implementation;
            var orderedTargets = selector.GetOrderedTargets(2000);
            foreach (var target in orderedTargets)
            {
                if (!remainingEnemies.Contains(target))
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
                    Storings.BARRELRANGE, Storings.Player.Position.To2D(), e.Range);
                if (!intersections.Any())
                {
                    continue;
                }
                List<Vector3> optimalPositions = new List<Vector3>(4);
                foreach (Vector2 vector in intersections)
                {
                    optimalPositions.Add(vector.To3D());
                }
                optimalPositions.Add(Storings.Player.Position.ReduceToMaxDistance(extCircle.Item1, e.Range));
                optimalPositions.Add(barrelPosition.ReduceToMaxDistance(extCircle.Item1, Storings.BARRELRANGE));
                dist = float.MaxValue;
                foreach (var position in optimalPositions)
                {
                    float dist2 = position.Distance(extCircle.Item1);
                    if (dist2 < dist && dist2 < extCircle.Item2)
                    {
                        barrelPosition = position;
                        dist = dist2;
                    }
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (dist != float.MaxValue)
                {
                    e.Cast(barrelPosition);
                    Console.WriteLine("Calculating optimal Extension Position took {0} ms", time);
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

        private List<Obj_AI_Hero> GetRemainingEnemies()
        {
            List<Obj_AI_Hero> toReturn = notHitEnemies.ToList();
            for (int i = toReturn.Count - 1; i >= 0; i--)
            {
                if (barrelsWithExplosionTimes.Any(barrelTimeTuple => bPrediction.CannotEscape(barrelTimeTuple.Item1,
                    toReturn[i], GetPredictionDelay(barrelTimeTuple.Item2))))
                {
                    toReturn.RemoveAt(i);
                }
            }
            return toReturn;
        }
    }
}