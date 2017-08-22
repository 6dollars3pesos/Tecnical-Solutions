﻿using System;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    public class Barrel : IComparable<Barrel>
    {
        #region Private Members
        //Saving Player Level here to prevent misbehaviour on Level up
        private readonly int playerLevel;

        private readonly SortedSet<int> attackTimes = new SortedSet<int>();

        private int barrelAttackTime;

        #endregion


        #region Public Members


        public Obj_AI_Minion BarrelObject { get; }
        
        #endregion
        
        
        #region Methods
        
        private static int GetBarrelAttackTime()
        {
            if (Storings.Player.Level < 7) return Game.TickCount + 4000;
            if (Storings.Player.Level < 13) return Game.TickCount + 2000;
            return Game.TickCount + 1000;
        }

        public Barrel(Obj_AI_Minion barrel)
        {
            BarrelObject = barrel;
            barrelAttackTime = GetBarrelAttackTime();
            playerLevel = Storings.Player.Level;
        }

        public void ReduceBarrelAttackTick()
        {
            barrelAttackTime -= getReducedTime();
        }

        public void ReduceBarrelAttackTick(int delay)
        {
            attackTimes.Add(Game.TickCount + delay);
            DelayAction.Queue(delay, () =>
            {
                attackTimes.Remove(attackTimes.Min);
                ReduceBarrelAttackTick();
            });
        }

        /// <summary>
        /// Returns whether the Player can use Q to destroy this Barrel
        /// <para>
        /// Includes Range Check
        /// </para> 
        /// </summary>
        /// <param name="delay">additional Delay</param>
        /// <returns>true if player destroys Barrel with Q</returns>
        public bool CanQNow(int delay = 0)
        {
            return CanDestroyAtTime(Helper.GetQTime(BarrelObject.Position) + delay + Game.TickCount);
        }

        /// <summary>
        /// Returns whether the Player can use an Autoattack to destroy this barrel
        /// <para>
        /// Does not Include Range Check
        /// </para>
        /// </summary>
        /// <returns>True if AA can be done</returns>
        public bool CanAANow(int delay = 0)
        {
            return CanDestroyAtTime(Game.TickCount + (int)Storings.Player.AttackCastDelay * 1000 + delay);
        }

        private bool CanDestroyAtTime(int tick)
        {
            int attackCount = attackTimes.Count(t => t <= tick);
            if (barrelAttackTime <= tick)
            {
                return attackCount == 0;
            }
            return barrelAttackTime - getReducedTime() * attackCount <= tick 
                && barrelAttackTime - getReducedTime() * (attackCount + 1) > tick;
        }

        private int getReducedTime()
        {
            if (playerLevel < 7) return 2000;
            if (playerLevel < 13) return 1000;
            return 500;
        }
        
        
        public int CompareTo(Barrel other)
        {
            return BarrelObject.NetworkId.CompareTo(other.BarrelObject.NetworkId);
        }
        #endregion
    }
}