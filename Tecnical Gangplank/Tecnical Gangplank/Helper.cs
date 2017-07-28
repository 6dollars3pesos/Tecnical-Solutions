﻿using System;
using Aimtec;
using Aimtec.SDK.Extensions;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    public static class Helper
    {
        public static int GetQTime(Vector3 position)
        {
            return (int) (Storings.Player.Distance(position) / 2.6f + Storings.QDELAY + Game.Ping / 2f);
        }

        /// <summary>
        /// Intersection of 2 Circles (used for optimal extension of double and triple Barrel)
        /// </summary>
        /// <param name="posA">Middle of Circle A</param>
        /// <param name="distA">Radius of Circle A</param>
        /// <param name="posB">Middle of Circle B</param>
        /// <param name="distB">Radius of Circle B</param>
        /// <returns>Array of 0, 1 or 2 intersection Points</returns>
        public static Vector2[] IntersectCircles(Vector2 posA, float distA, Vector2 posB, float distB)
        {
            var ab0 = posB.X - posA.X;
            var ab1 = posB.Y - posA.Y;
            var c = Math.Sqrt(ab0 * ab0 + ab1 * ab1);
            if (c == 0)
            {
                // no distance between centers
                return new Vector2[0];
            }
            
                        var x = (distA * distA + c * c - distB * distB) / (2 * c);
            var y = distA * distA - x * x;
            if (y < 0)
            {
                // no intersection
                return new Vector2[0];
            }
            if (y > 0)
            {
                y = Math.Sqrt(y);
            }
            // compute unit vectors ex and ey
            var ex0 = ab0 / c;
            var ex1 = ab1 / c;
            var ey0 = -ex1;
            var ey1 = ex0;
            var q1x = (float)(posA.X + x * ex0);
            var q1y = (float)(posA.Y + x * ex1);
            if (y == 0)
            {
                // one touch point
                return new[]{new Vector2(q1x, q1y)};
            }
            // two intersections
            var q2x = (float)(q1x - y * ey0);
            var q2y = (float)(q1y - y * ey1);
            q1x += (float)(y * ey0);
            q1y += (float)(y * ey1);
            return new[] { new Vector2(q1x, q1y), new Vector2(q2x, q2y)};
        }
    }
}