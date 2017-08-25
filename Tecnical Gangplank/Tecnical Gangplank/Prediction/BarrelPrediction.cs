﻿using System;
using System.Collections.Generic;
 using System.Drawing;
 using System.Linq;
using Aimtec;
 using Aimtec.SDK.Events;
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
            if (Storings.MenuConfiguration.MiscDebug.Value)
            {
                Render.OnPresent += DrawPrediction;
            }
            barrelManager = manager;
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                enemies.Add(new PredictionPlayer(enemy));
            }
        }

        private void DrawPrediction()
        {
            if (!Storings.MenuConfiguration.MiscDebug.Value)
            {
                return;
            }
            foreach (PredictionPlayer predictionEnemy in enemies)
            {
                Render.Circle(GetPositionAfterTime(predictionEnemy.Hero, GetReactionTime(predictionEnemy)), 50, 180, Color.DarkOrange);
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
            
            if (delay < 0)
            {
                return false;
            }
            int completeReactionTime = GetReactionTime(enemies.Find(e => e.Hero == enemy));
            Vector3 predictedEnemyPosition = GetPositionAfterTime(enemy, (int)Math.Min(completeReactionTime, delay));
            if (predictedEnemyPosition.Distance(barrel.BarrelObject.Position) < Storings.BARRELRANGE
                - Storings.PREDICTIONMODIFIER * Math.Max(delay - completeReactionTime, 0) * enemy.MoveSpeed * 0.001)
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
                                       enemy.MoveSpeed * Storings.PREDICTIONMODIFIER * 0.001f *
                                       (Math.Max(delay - completeReactionTime, 0) + Storings.CHAINTIME * tuple.Item2);
                if (remainingRange < 0)
                {
                    return false;
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
            int completeReationTime = GetReactionTime(enemies.Find(e => e.Hero == enemy));
            Vector3 predictedEnemyPosition =
                GetPositionAfterTime(enemy, Math.Min(completeReationTime, delay));

            return predictedEnemyPosition.Distance(barrel.BarrelObject.Position) < Storings.BARRELRANGE
                   - Storings.PREDICTIONMODIFIER * Math.Min(delay - completeReationTime, 0) * enemy.MoveSpeed
                   && enemy.Position.Distance(barrel.BarrelObject) < Storings.BARRELRANGE;
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
            int[] reactionTimes = new int[3];
            //Remaining Dash Time
            reactionTimes[0] =0;// enemy.CurrentDash.EndTick - Game.TickCount;
            //Generic Reaction Time
            reactionTimes[1] = reactionTime + Math.Max(additionalReactionTime
                                               + enemies.Find(e => e == enemy).LastPositionChange - Game.TickCount, 0);
            try
            {
                int snareTime = enemy.Hero.Buffs
                    .Where(b => b.Type == BuffType.Snare || b.Type == BuffType.Stun || b.Type == BuffType.Knockup)
                    .Max(b => (int) ((b.EndTime - Game.ClockTime) * 1000));
                //Increased Reaction time if player has Stun/Snare/Knockup Debuff
                reactionTimes[2] = snareTime;
            }
            catch (InvalidOperationException)
            {
                //Intended Behaviour when Player is not Debuffed
            }
            return reactionTimes.Max();

        }
        
                
        private Vector3 GetPositionAfterTime(Obj_AI_Hero enemy, int ticks)
        {
            PredictionPlayer predEnemy = enemies.Find(e => e.Hero == enemy);
            if (predEnemy.isDashing())
            {
                return predEnemy.CurrentDash.EndTick > ticks + Game.TickCount 
                    ? enemy.Position.Extend(predEnemy.CurrentDash.EndPos.To3D(), 
                        ticks * predEnemy.CurrentDash.Speed * 0.001f)
                    : predEnemy.CurrentDash.EndPos.To3D();
            }
            if (!enemy.HasPath)
            {
                return enemy.Position;
            }
            Vector3 priorPosition = enemy.Position;
            float movementPending = ticks * enemy.MoveSpeed * 0.001f;
            bool firstPosition = true;
            foreach (Vector3 nextPosition in enemy.Path)
            {
                if (firstPosition)
                {
                    firstPosition = false;
                    continue;
                }
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
            internal Dash.DashArgs CurrentDash;
                        
            internal readonly Obj_AI_Hero Hero;

            internal int LastPositionChange;
            
            internal PredictionPlayer(Obj_AI_Hero hero)
            {
                Hero = hero;
                LastPositionChange = Game.TickCount;
                Obj_AI_Base.OnNewPath += UpdatePosition;
                Dash.HeroDashed += HeroDash;
            }

            internal bool isDashing()
            {
                return CurrentDash != null && CurrentDash.EndTick > Game.TickCount;
            }

            private void HeroDash(object sender, Dash.DashArgs dashArgs)
            {
                if (dashArgs.Unit != Hero)
                {
                    return;
                }
                LastPositionChange = Game.TickCount;
                if (dashArgs.IsBlink)
                {
                    return;
                }
                CurrentDash = dashArgs;
            }

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
