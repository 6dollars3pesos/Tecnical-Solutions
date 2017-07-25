using System.Collections.Generic;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Orbwalking;

namespace TecnicalGangplank.Configurations
{
    public class Config
    {
        public IMenu FullMenu { get; }
        public IOrbwalker Orbwalker { get; }

        #region Menu Getters

        public MenuBool ComboQ { get; }
        
        public MenuBool ComboQBarrel { get; }
        
        public MenuBool ComboAABarrel { get; }
        
        public MenuBool ComboE { get; }
        
        public MenuBool ComboEExtend { get; }
        
        public MenuBool ComboDoubleE { get; }
        
        public MenuBool ComboTripleE { get; }
        
        public MenuBool MiscExtendE { get; }
        
        public MenuSlider MiscReactionTime { get; }
        
        public MenuBool LastHitBarrelQ { get; }
        
        public MenuBool LastHitQ { get; }
        
        public MenuSlider LastHitMinimumQ { get; }

        private readonly Dictionary<BuffType, MenuBool> enabledBuffs = new Dictionary<BuffType, MenuBool>
        {
            {BuffType.Blind, null},
            {BuffType.Stun, null},
            {BuffType.Fear, null},
            {BuffType.Taunt, null},
            {BuffType.Poison, null},
            {BuffType.Slow, null},
            {BuffType.Suppression, null},
            {BuffType.Silence, null},
            {BuffType.Snare, null}
        };

        
        #endregion
        
        public Config()
        {
            Orbwalker = Aimtec.SDK.Orbwalking.Orbwalker.Implementation;

            FullMenu = new Menu("tecgp.root","Technical Gangplank",true);
            {
                Orbwalker.Attach(FullMenu);
            }
            {
                Menu spellMenu = new Menu("tecgp.combo", "Combo");
                ComboQ = new MenuBool("tecgp.combo.q", "Use Q");
                ComboQBarrel = new MenuBool("tecgp.combo.qe", "Use Q on Barrel");
                ComboAABarrel = new MenuBool("tecgp.combo.aae", "Use Autoattack on Barrel");
                ComboE = new MenuBool("tecgp.combo.e", "Place first E", false);
                ComboEExtend = new MenuBool("tecgp.combo.ex", "Use E to Chain");
                ComboDoubleE = new MenuBool("tecgp.combo.doublee", "Use Double E Combo", false);
                ComboDoubleE.SetToolTip("Requires low Ping");
                ComboTripleE = new MenuBool("tecgp.combo.triplee", "Use Triple E Combo");
                //Todo Add R
                spellMenu.Add(ComboQ);
                spellMenu.Add(ComboQBarrel);
                spellMenu.Add(ComboAABarrel);
                spellMenu.Add(ComboE);
                spellMenu.Add(ComboEExtend);
                spellMenu.Add(ComboDoubleE);
                spellMenu.Add(ComboTripleE);
                FullMenu.Add(spellMenu);
            }
            {
                Menu cleanseMenu = new Menu("tecgp.cleanse", "Cleansing");
                foreach (BuffType cBuff in enabledBuffs.Keys)
                {
                    enabledBuffs[cBuff] = new MenuBool("tecgp.cleanse." + cBuff, cBuff.ToString());
                    cleanseMenu.Add(enabledBuffs[cBuff]);
                }
                FullMenu.Add(cleanseMenu);
            }
            {
                Menu lastHitMenu = new Menu("tecgp.lasthit", "Lasthit");
                
                FullMenu.Add(lastHitMenu);
            }
            {
                Menu miscMenu = new Menu("tecgp.misc", "Misc");
                MiscExtendE = new MenuBool("tecgp.misc.ex", "Extend E for additional Hits");
                MiscReactionTime = new MenuSlider("tecgp.misc.reactiontime", 
                    "Enemy Reaction Time in ms (for Prediction)", 90, 0, 200);
                
                miscMenu.Add(MiscExtendE);
                miscMenu.Add(MiscReactionTime);
                FullMenu.Add(miscMenu);
            }
            FullMenu.Attach();
        }
    }
}