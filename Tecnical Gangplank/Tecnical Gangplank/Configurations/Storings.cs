﻿using Aimtec;
using Aimtec.SDK.TargetSelector;
using TecnicalGangplank.Logic;
using Spell = Aimtec.SDK.Spell;

namespace TecnicalGangplank.Configurations
{
    internal static class Storings
    {
        public const int QDELAY = 150; //Lower = Higher Accuracy
        public const int EXECUTION_OFFSET = 300;
        public const float CONNECTRANGE = 685;
        public const int CHAINTIME = 350;
        public const int BARRELAARANGE = 225;
        public const string BARRELNAME = "Barrel";
        public const float BARRELRANGE = CONNECTRANGE / 2;
        public const float PREDICTIONMODIFIER = 0.8f;
        public static readonly Config MenuConfiguration = new Config();
        public static readonly Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static readonly ITargetSelector Selector = TargetSelector.Implementation;
        public static readonly Champion ChampionImpl = new Gangplank();
    }
}