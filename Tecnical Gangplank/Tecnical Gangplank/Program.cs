﻿using System;
using System.Collections.Generic;
using Aimtec;
using Aimtec.SDK.Events;
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
            Console.WriteLine(Storings.Player.ChampionName.ToLower());
            if (Storings.Player.ChampionName.ToLower() != "gangplank")
            {
                Console.WriteLine(Storings.Player.ChampionName.ToLower());
                return;
            }
            //Aimtec.SDK.Bootstrap.Load();
            Storings.CHAMPIONIMPL.HandleGameLoad();
        }
    }
}