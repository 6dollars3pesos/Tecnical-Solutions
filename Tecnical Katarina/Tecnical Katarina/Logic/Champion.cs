using Aimtec;
using TecnicalKatarina.Configurations;
using Spell = Aimtec.SDK.Spell;

namespace TecnicalKatarina.Logic
{
    public abstract class Champion
    {        
        #region Properties

        public Spell Q { get; }

        public Spell W { get; }

        public Spell E { get; }

        public Spell R { get; }
                
        #endregion
        
        protected Champion(float[] ranges)
        {
            if (ranges.Length != 4)
            {
                throw new TecnicalException("Champion could not be initialized - Improper Constructor call");
            }
            Q = new Spell(SpellSlot.Q, ranges[0]);
            W = new Spell(SpellSlot.W, ranges[1]);
            E = new Spell(SpellSlot.E, ranges[2]);
            R = new Spell(SpellSlot.R, ranges[3]);
        }

        public abstract void UpdateGame();

        public void HandleGameLoad()
        {
            InternalGameLoad();
            LoadGame();
        }

        public abstract void LoadGame();

        private void InternalGameLoad()
        {
            Game.OnUpdate += UpdateGame;
            Obj_AI_Base.OnProcessSpellCast += (sender, e) =>
            {
                if (sender.IsMe)
                {
                    ProcessPlayerCast(sender, e);
                }
            };
            Obj_AI_Base.OnProcessAutoAttack += (sender, e) =>
            {
                if (sender.IsMe)
                {
                    ProcessPlayerAutoAttack(sender, e);
                }
            };
        }

        protected abstract void ProcessPlayerCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e);
        protected abstract void ProcessPlayerAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e);
    }
}