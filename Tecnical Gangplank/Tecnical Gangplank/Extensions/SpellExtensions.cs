using Aimtec;
using TecnicalGangplank.Configurations;

namespace TecnicalGangplank.Extensions
{
    public static class SpellExtensions
    {
        public static Spell GetSpell(this Aimtec.SDK.Spell spell)
        {
            return Storings.Player.SpellBook.GetSpell(spell.Slot);
        }
    }
}