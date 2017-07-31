﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;
 using Aimtec.SDK.Util.Cache;
 using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    public class BarrelManager
    {
        private SortedSet<Barrel> Barrels { get; }

        public BarrelManager()
        {
            Barrels = new SortedSet<Barrel>();
            foreach (Obj_AI_Minion minion in GameObjects.EnemyMinions)
            {
                AddBarrel(minion);
            }
            GameObject.OnCreate += AddBarrel;
            //Not beautiful/efficient, but reliable
            Game.OnUpdate += RemoveBarrel;
            Obj_AI_Base.OnProcessAutoAttack += ReduceTicks;
        }

        private void ReduceTicks(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            GameObject target = args.Target;
            if (target == null || !target.IsMinion || target.Name != Storings.BARRELNAME)
            {
                return;
            }
            Obj_AI_Minion barrel = target as Obj_AI_Minion;
            Barrel toReduce = Barrels.FirstOrDefault(b => b.BarrelObject == barrel);
            if (toReduce == null)
            {
                return;
            }
            if (args.Sender.IsMelee)
            {
                toReduce.ReduceBarrelAttackTick();
            }
            else
            {
                toReduce.ReduceBarrelAttackTick((int)(args.Start.Distance(args.End)
                                                      /args.SpellData.MissileSpeed));
            }
        }

        private void AddBarrel(GameObject barrel)
        {
            if (barrel == null || !barrel.IsMinion || barrel.Name != Storings.BARRELNAME)
            {
                return;
            }
            Barrels.Add(new Barrel((Obj_AI_Minion)barrel));
        }
        
        
        
        private void RemoveBarrel()
        {
            Barrels.RemoveWhere(b => b.BarrelObject.Health < 1);
        }
        
        
        
        /// <summary>
        /// Gets all Barrels in Range of the current Barrel
        /// <para>Does not include the actual Barrel itself</para>
        /// </summary>
        /// <param name="barrel">Barrel with barrels around</param>
        /// <param name="range">Requested Range (default Maxrange)</param>
        /// <returns>All Barrels in Range</returns>
        public IEnumerable<Barrel> GetBarrelsInRange(Barrel barrel, float range = Storings.CONNECTRANGE)
        {
            return Barrels.Where(b => b != barrel 
                && b.BarrelObject.Distance(barrel.BarrelObject) <= range);
        }

        public IEnumerable<Barrel> GetBarrelsInRange(float range)
        {
            return Barrels.Where(b => b.BarrelObject.Distance(Storings.Player) <= range);
        }
        
        public IEnumerable<Barrel> GetBarrelsInRange(Vector3 pos, float range)
        {
            return Barrels.Where(b => b.BarrelObject.Distance(pos) <= range);
        }

        public Barrel GetNearestBarrel(Vector3 pos)
        {
            return Barrels.MinBy(b => b.BarrelObject.Distance(pos));
        }

        public IEnumerable<Barrel> GetBarrels()
        {
            return Barrels;
        }

        
        

        public IEnumerable<Tuple<Barrel, int>> GetBarrelsWithBounces(Obj_AI_Minion initObj)
        {
            return GetBarrelsWithBounces(Barrels.First(b => b.BarrelObject == initObj));
        }

        /// <summary>
        /// Gets all Barrels that are chained to the current Barrel
        /// <para>Does include the Barrel itself</para>
        /// </summary>
        /// <param name="initObj">Object in the middle</param>
        /// <returns>All Barrels with number of Chains</returns>
        public IEnumerable<Tuple<Barrel, int>> GetBarrelsWithBounces(Barrel initObj)
        {
            Barrel[] barrels = Barrels.ToArray();
            bool[] alreadyused = new bool[barrels.Length];
            for (int i = 0; i < barrels.Length; i++)
            {
                if (barrels[i] == initObj)
                {
                    alreadyused[i] = true;
                    break;
                }
            }
            List<Tuple<Barrel, int>> toReturn = new List<Tuple<Barrel, int>> 
                {new Tuple<Barrel, int>(initObj, 0)};
            
            bool foundObjects = true;
            int cind = 0;
            while (foundObjects)
            {
                cind++;
                foundObjects = false;
                for (int i = 0; i < alreadyused.Length; i++)
                {
                    if (alreadyused[i])
                    {
                        continue;
                    }
                    for (int j = toReturn.Count - 1; j >= 0; j--)
                    {
                        if (toReturn[j].Item2 < cind - 1)
                        {
                            break;
                        }
                        if (toReturn[j].Item2 > cind - 1)
                        {
                            continue;
                        }
                        if (barrels[i].BarrelObject.Distance(toReturn[j].Item1.BarrelObject) <= Storings.CONNECTRANGE)
                        {
                            alreadyused[i] = true;
                            foundObjects = true;
                            toReturn.Add(new Tuple<Barrel, int>(barrels[i],cind));
                        }
                    }
                }
            }
            return toReturn;
        }
    }
}