using System;
using System.Collections.Generic;
using Aimtec;
using Aimtec.SDK.Events;
using Aimtec.SDK.Util.Cache;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            GameEvents.GameStart += Initialize;
        }

        private static void Initialize()
        {
            if (GameObjects.Player.ChampionName.ToLower() != "gangplank")
            {
                return;
            }
            //Aimtec.SDK.Bootstrap.Load();
            LoadChampion();
        }

        private static void LoadChampion()
        {
            Storings.ChampionImpl.HandleGameLoad();
        }
    }
}