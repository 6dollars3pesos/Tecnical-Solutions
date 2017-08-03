using System;
using System.Collections.Generic;
using Aimtec;
using Aimtec.SDK.Events;
using Aimtec.SDK.Util.Cache;
using TecnicalKatarina.Configurations;

namespace TecnicalKatarina
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            GameEvents.GameStart += Initialize;
        }

        private static void Initialize()
        {
            if (GameObjects.Player.ChampionName.ToLower() != "katarina")
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