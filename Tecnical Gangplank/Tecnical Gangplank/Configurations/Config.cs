﻿using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Orbwalking;
 using Aimtec.SDK.Util;

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
        
        public MenuBool KillStealQ { get; }
        
        public MenuBool KillStealR { get; }
        
        public MenuBool KeyDetonation { get; }
        
        public MenuKeyBind KeyDetonationKey { get; }
        
        public MenuBool KeyDetonationOrbwalk { get; }

        public Dictionary<BuffType, MenuBool> EnabledBuffs = new Dictionary<BuffType, MenuBool>
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
                Menu keysMenu = new Menu("tecgp.keys", "Keys");
                KeyDetonation = new MenuBool("tecgp.keys.detonation", "Extend Barrel to mouse and detonate first", false);
                KeyDetonationKey = new MenuKeyBind("tecgp.keys.detonationkey", "Key for Extending Barrel", KeyCode.T, KeybindType.Press);
                KeyDetonationOrbwalk = new MenuBool("tecgp.keys.detonationorbwalk", "Orbwalk on Detonation");
                keysMenu.Add(KeyDetonation);
                keysMenu.Add(KeyDetonationKey);
                FullMenu.Add(keysMenu);
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
                foreach (BuffType cBuff in EnabledBuffs.Keys.ToArray())
                {
                    EnabledBuffs[cBuff] = new MenuBool("tecgp.cleanse." + cBuff, cBuff.ToString());
                    cleanseMenu.Add(EnabledBuffs[cBuff]);
                }
                FullMenu.Add(cleanseMenu);
            }
            {
                Menu lastHitMenu = new Menu("tecgp.lasthit", "Lasthit");
                LastHitBarrelQ = new MenuBool("tecgp.lasthit.barrelq", "Q on Barrels");
                LastHitMinimumQ = new MenuSlider("tecgp.lasthit.minimumq", "Minimum Minions to Q on Barrel", 2, 1, 8);
                LastHitQ = new MenuBool("tecgp.lasthit.q", "Q on Minions");
                
                lastHitMenu.Add(LastHitBarrelQ);
                lastHitMenu.Add(LastHitMinimumQ);
                lastHitMenu.Add(LastHitQ);
                FullMenu.Add(lastHitMenu);
            }
            {
                Menu killStealMenu = new Menu("tecgp.ks", "Killsteal");
                KillStealQ = new MenuBool("tecgp.killsteal.q", "Use Q");
                KillStealR = new MenuBool("tecgp.killsteal.r", "Use R", false);

                killStealMenu.Add(KillStealQ);
                killStealMenu.Add(KillStealR);
                FullMenu.Add(killStealMenu);
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