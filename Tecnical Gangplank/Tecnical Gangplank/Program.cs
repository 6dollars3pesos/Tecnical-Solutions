using System;
using System.Collections.Generic;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (Storings.Player.ChampionName.ToLower() != "gangplank")
            {
                return;
            }
            Aimtec.SDK.Bootstrap.Load();
        }
    }
}