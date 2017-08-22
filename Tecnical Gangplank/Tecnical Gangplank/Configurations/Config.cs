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
        
        public MenuSlider ComboEMinimum { get; }
        
        public MenuBool ComboEExtend { get; }
        
        public MenuBool ComboDoubleE { get; }
        
        public MenuBool ComboTripleE { get; }
        
        public MenuBool MiscExtendE { get; }
        
        public MenuSlider MiscReactionTime { get; }

        public MenuSlider MiscChainCorrection { get; }
        
        public MenuBool MiscDynamicTargetRange { get; }
        
        public MenuSlider MiscAdditionalReactionTime { get; }

        public MenuBool MiscDebug { get; }
        
        public MenuBool LastHitBarrelQ { get; }
        
        public MenuBool LastHitQ { get; }
        
        public MenuSlider LastHitMinimumQ { get; }
        
        public MenuBool LaneClearBarrelQ { get; }
        
        public MenuBool LaneClearQ { get; }
        
        public MenuSlider LaneClearMinimumQ { get; }
        
        public MenuBool KillStealQ { get; }
        
        public MenuBool KillStealR { get; }
        
        public MenuBool KeyDoDetonation { get; }
        
        public MenuKeyBind KeyDetonationKey { get; }
        
        public MenuBool KeyDetonationOrbwalk { get; }
        
        public MenuBool KeyDoExplodeNextBarrel { get; }
        
        public MenuKeyBind KeyExplodeNextBarrelKey { get; }
        
        public MenuBool DrawQ { get; }
                
        public MenuBool DrawE { get; }
        
        public MenuBool DrawConnectionRange { get; }
        
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
                KeyDoDetonation = new MenuBool("tecgp.keys.detonation", "Extend Barrel to mouse and detonate first", false);
                KeyDetonationKey = new MenuKeyBind("tecgp.keys.detonationkey", "Key for Extending Barrel", KeyCode.T, KeybindType.Press);
                KeyDetonationOrbwalk = new MenuBool("tecgp.keys.detonationorbwalk", "Orbwalk on Detonation");
                KeyDoExplodeNextBarrel = new MenuBool("tecgp.keys.explodebarrel", "Explode nearest Barrel", false);
                KeyExplodeNextBarrelKey = new MenuKeyBind("tecgp.keys", "Key for exploding nearest Barrel", KeyCode.Z, KeybindType.Press);
                
                keysMenu.Add(KeyDoDetonation);
                keysMenu.Add(KeyDetonationKey);
                keysMenu.Add(KeyDetonationOrbwalk);
                keysMenu.Add(KeyDoExplodeNextBarrel);
                keysMenu.Add(KeyExplodeNextBarrelKey);
                FullMenu.Add(keysMenu);
            }
            {
                Menu spellMenu = new Menu("tecgp.combo", "Combo");
                ComboQ = new MenuBool("tecgp.combo.q", "Use Q");
                ComboQBarrel = new MenuBool("tecgp.combo.qe", "Use Q on Barrel");
                ComboAABarrel = new MenuBool("tecgp.combo.aae", "Use Autoattack on Barrel");
                ComboEMinimum = new MenuSlider("tecgp.combo.e", "Minimum Ammo to place first E", 4, 1, 4);
                ComboEMinimum.SetToolTip("Set to 4 to turn off");
                ComboEExtend = new MenuBool("tecgp.combo.ex", "Use E to Chain");
                ComboDoubleE = new MenuBool("tecgp.combo.doublee", "Use Double E Combo", false);
                ComboDoubleE.SetToolTip("Requires low Ping");
                ComboTripleE = new MenuBool("tecgp.combo.triplee", "Use Triple E Combo");
                spellMenu.Add(ComboQ);
                spellMenu.Add(ComboQBarrel);
                spellMenu.Add(ComboAABarrel);
                spellMenu.Add(ComboEMinimum);
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
                Menu laneClearMenu = new Menu("tecgp.laneclear", "Lasthit");
                LaneClearBarrelQ = new MenuBool("tecgp.laneclear.barrelq", "Q on Barrels");
                LaneClearMinimumQ = new MenuSlider("tecgp.laneclear.minimumq", "Minimum Minions to Q on Barrel", 2, 1, 8);
                LaneClearQ = new MenuBool("tecgp.laneclear.q", "Q on Minions");
                
                laneClearMenu.Add(LaneClearBarrelQ);
                laneClearMenu.Add(LaneClearMinimumQ);
                laneClearMenu.Add(LaneClearQ);
                FullMenu.Add(laneClearMenu);
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
                Menu drawingsMenu = new Menu("tecgp.draw", "Drawings");
                DrawQ = new MenuBool("tecgp.drawq", "Draw Q Range", false);
                DrawE = new MenuBool("tecgp.drawe", "Draw E Range", false);
                DrawConnectionRange = new MenuBool("tecgp.drawconnectionrange", "Draw Barrel Connection Range", false);

                drawingsMenu.Add(DrawQ);
                drawingsMenu.Add(DrawE);
                drawingsMenu.Add(DrawConnectionRange);
                FullMenu.Add(drawingsMenu);
            }
            {
                Menu miscMenu = new Menu("tecgp.misc", "Misc");
                MiscExtendE = new MenuBool("tecgp.misc.ex", "Extend E for additional Hits");
                MiscReactionTime = new MenuSlider("tecgp.misc.reactiontime", 
                    "Enemy Reaction Time in ms (for Prediction)", 90, 0, 200);
                MiscAdditionalReactionTime = new MenuSlider("tecgp.misc.additionalreacttime", 
                    "Additional Reaction Time in ms (for Prediction)", 50);
                MiscChainCorrection = new MenuSlider("tecgp.misc.chaincorrection", "Autochain when x out of range", 100, 0, 300);
                MiscDynamicTargetRange = new MenuBool("tecgp.misc.dyntargetrange", "Dynamic Target for each Spell");
                MiscDebug = new MenuBool("tecgp.misc.debug", "Debug", false);

                miscMenu.Add(MiscExtendE);
                miscMenu.Add(MiscReactionTime);
                miscMenu.Add(MiscAdditionalReactionTime);
                miscMenu.Add(MiscChainCorrection);
                miscMenu.Add(MiscDynamicTargetRange);
                miscMenu.Add(MiscDebug);
                FullMenu.Add(miscMenu);
            }
            FullMenu.Attach();
        }
    }
}