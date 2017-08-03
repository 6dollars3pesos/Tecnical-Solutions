﻿using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank.Prediction
{
    public class BarrelPrediction
    {
        private readonly List<PredictionPlayer> enemies = new List<PredictionPlayer>();

        private readonly BarrelManager barrelManager;

        private int reactionTime => Storings.MenuConfiguration.MiscReactionTime.Value;

        private int additionalReactionTime => Storings.MenuConfiguration.MiscAdditionalReactionTime.Value;
        
        public BarrelPrediction(BarrelManager manager)
        {
            barrelManager = manager;
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                enemies.Add(new PredictionPlayer(enemy));
            }
        }

        /// <summary>
        /// Checks if a Player can hit the Enemy by hitting a Barrel with the specific Delay
        /// <para>
        /// Takes the time for chained Barrels in Account
        /// </para>
        /// </summary>
        /// <param name="barrel">Barrel to use</param>
        /// <param name="enemy">Enemy to hit</param>
        /// <param name="delay">Delay until attacking Barrel</param>
        /// <returns>True if Player can get that Player with a Barrel</returns>
        public bool CanHitEnemy(Barrel barrel, Obj_AI_Hero enemy, float delay)
        {
            int completeReactionTime = GetReactionTime(enemies.Find(e => e.Hero == enemy));

            Vector3 predictedEnemyPosition = GetPositionAfterTime(enemy, completeReactionTime);
            if (predictedEnemyPosition.Distance(barrel.BarrelObject.Position) < Storings.BARRELRANGE
                - Storings.PREDICTIONMODIFIER * Math.Min(delay - completeReactionTime, 0) * enemy.MoveSpeed)
            {
                return true;
            }

            foreach (var tuple in barrelManager.GetBarrelsWithBounces(barrel))
            {
                if (tuple.Item2 == 0)
                {
                    continue;
                }
                float remainingRange = Storings.BARRELRANGE -
                                       enemy.MoveSpeed * Storings.PREDICTIONMODIFIER *
                                       (Math.Min(delay - completeReactionTime, 0) + Storings.CHAINTIME * tuple.Item2);
                if (remainingRange < 0)
                {
                    continue;
                }
                if (predictedEnemyPosition.Distance(tuple.Item1.BarrelObject.Position) < remainingRange)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the Player will be hit by that Barrel after a specific delay
        /// <para>
        /// Does not take chained Barrels in Account
        /// </para>
        /// </summary>
        /// <param name="barrel">The Barrel</param>
        /// <param name="enemy">The Enemy</param>
        /// <param name="delay">Delay until evaluating</param>
        /// <returns>True if enemy cannot escape</returns>
        public bool CannotEscape(Barrel barrel, Obj_AI_Hero enemy, int delay)
        {
            Console.WriteLine("Distance: {0}, Delay: {1}", barrel.BarrelObject.Distance(enemy), delay);
            int completeReationTime = GetReactionTime(enemies.Find(e => e.Hero == enemy));
            
            Vector3 predictedEnemyPosition =
                GetPositionAfterTime(enemy, completeReationTime);

            return predictedEnemyPosition.Distance(barrel.BarrelObject.Position) < Storings.BARRELRANGE
                   - Storings.PREDICTIONMODIFIER * Math.Min(delay - completeReationTime, 0) * enemy.MoveSpeed;
        }

        public Tuple<Vector3, float> GetPredictionCircle(Obj_AI_Hero enemy, int delay)
        {
            int completeReationTime = GetReactionTime(enemies.Find(e => e.Hero == enemy));
            return new Tuple<Vector3, float>(GetPositionAfterTime(enemy, completeReationTime),
                Storings.BARRELRANGE - Storings.PREDICTIONMODIFIER 
                * Math.Min(delay - completeReationTime, 0) * enemy.MoveSpeed);
        }

        private int GetReactionTime(PredictionPlayer enemy)
        {
            return reactionTime + Math.Max(additionalReactionTime 
                  + enemies.Find(e => e == enemy).LastPositionChange - Game.TickCount, 0);
        }
        
                
        private Vector3 GetPositionAfterTime(Obj_AI_Hero enemy, int ticks)
        {
            if (!enemy.HasPath)
            {
                return enemy.Position;
            }
            Vector3 priorPosition = enemy.Position;
            float movementPending = ticks * enemy.MoveSpeed * 0.001f;
            foreach (Vector3 nextPosition in enemy.Path)
            {
                float vectorLength = (priorPosition - nextPosition).Length;
                if (vectorLength > movementPending)
                {
                    return priorPosition.Extend(nextPosition, movementPending);
                }
                movementPending -= vectorLength;
                priorPosition = nextPosition;
            }
            return priorPosition;
        }

        public Vector3 GetPredictedPosition(Obj_AI_Hero enemy)
        {
            return GetPositionAfterTime(enemy, GetReactionTime(enemies.Find(e => e.Hero == enemy)));
        }

        private class PredictionPlayer
        {
            internal readonly Obj_AI_Hero Hero;

            internal int LastPositionChange;
            
            internal PredictionPlayer(Obj_AI_Hero hero)
            {
                Hero = hero;
                LastPositionChange = Game.TickCount;
                Obj_AI_Base.OnNewPath += UpdatePosition;
            }

            //Todo add Dashes
            private void UpdatePosition(Obj_AI_Base sender, Obj_AI_BaseNewPathEventArgs eventArgs)
            {
                if (Hero != sender)
                {
                    return;
                }
                LastPositionChange = Game.TickCount;
            }
        }
    }
}