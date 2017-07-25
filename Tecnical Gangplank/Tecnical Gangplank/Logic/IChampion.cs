using Aimtec.SDK;

namespace TecnicalGangplank.Logic
{
    public interface IChampion
    {
        #region Properties

        Spell Q { get; }
        
        Spell W { get; }
        
        Spell E { get; }
        
        Spell R { get; }
        
        #endregion
        
        void UpdateGame();

        void LoadGame();
    }
}