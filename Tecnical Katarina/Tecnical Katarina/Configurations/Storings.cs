using Aimtec;
using Aimtec.SDK.TargetSelector;
using TecnicalKatarina.Logic;

namespace TecnicalKatarina.Configurations
{
    internal static class Storings
    {
        public const int QMINIONDISTANCE = 350;
        public const int DAGGERMAXACTIVETIME = 4000;
        public const int DAGGERTRIGGERRANGE = 140;
        public const int DAGGERDAMAGERANGE = 300;
        public const string QDELETION = "PickUp";
        public const string RBUFFNAME = "katarinarsound";
        public static readonly string[] QCreationObjects = 
            {"Katarina_Base_E_Beam.troy", "Katarina_Base_Q_Dagger_Land_Dirt.troy"};
        public static readonly float[] PassiveDamages =
            {75f, 80, 87, 94, 102, 111, 120, 131, 143, 155, 168, 183, 198, 214, 231, 248, 267, 287};
        public static readonly Config MenuConfiguration = new Config();
        public static readonly Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static readonly ITargetSelector Selector = TargetSelector.Implementation;
        public static readonly Champion ChampionImpl = new Katarina();
        public static readonly DaggerManager DaggerManager = new DaggerManager();
    }
}