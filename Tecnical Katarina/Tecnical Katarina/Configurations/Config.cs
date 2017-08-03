using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Orbwalking;

namespace TecnicalKatarina.Configurations
{
    public class Config
    {
        public IMenu FullMenu { get; }
        
        public IOrbwalker Orbwalker { get; }
        
        public MenuBool ComboQ { get; }
        
        public MenuBool ComboQMinions { get; }
        
        public MenuBool ComboQDirect { get; }
        
        public MenuBool ComboQOnlyRunAway { get; }
        
        public MenuBool ComboW { get; }
        
        public MenuBool ComboE { get; }
        
        public MenuBool ComboEAlways { get; }
        
        public MenuBool ComboR { get; }
        
        public MenuSlider ComboRHealth { get; }
        
        public MenuBool HarassQ { get; }
        
        public MenuBool HarassW { get; }
        
        public MenuBool HarassE { get; }
        
        public MenuBool LastHitQ { get; }
                
        public MenuBool LastHitE { get; }
        
        public MenuBool LaneClearQ { get; }
        
        public MenuBool LaneClearW { get; }
        
        public MenuBool LaneClearE { get; }

        public MenuBool JungleClearQ { get; }
        
        public MenuBool JungleClearW { get; }
        
        public MenuBool JungleClearE { get; }
        
        public MenuBool DrawDagger { get; }
        
        public MenuBool DrawQ { get; }
                
        public MenuBool DrawE { get; }
        
        public MenuBool DrawR { get; }
        
        public MenuBool KillSteal { get; }
        
        public MenuBool KillStealDisturbR { get; }
        
        public MenuBool MiscPreventRCancel { get; }

        public Config()
        {
            FullMenu = new Menu("teckata", "Tecnical Katarina", true);
            {
                Orbwalker = Aimtec.SDK.Orbwalking.Orbwalker.Implementation;
                Orbwalker.Attach(FullMenu);
            }
            {
                Menu comboMenu = new Menu("teckata.combo", "Combo");
                {
                    Menu qOptionsMenu = new Menu("teckata.qmenu", "Q Options");
                    ComboQMinions = new MenuBool("teckata.q.minion", "Use Q on Minions");
                    ComboQDirect = new MenuBool("teckata.q.direct", "Use Q directly on Enemy");
                    ComboQOnlyRunAway = new MenuBool("teckata.q.onlyrunaway", 
                        "Use Q only when enemy does not face you", false);
                    qOptionsMenu.Add(ComboQMinions);
                    qOptionsMenu.Add(ComboQDirect);
                    qOptionsMenu.Add(ComboQOnlyRunAway);
                    comboMenu.Add(qOptionsMenu);
                }
                ComboQ = new MenuBool("teckata.combo.q", "Use Q");
                ComboW = new MenuBool("teckata.combo.w", "Use W");
                ComboE = new MenuBool("teckata.combo.e", "Use E");
                ComboEAlways = new MenuBool("teckata.combo.ealways", "Use E always");
                ComboR = new MenuBool("teckata.combo.r", "Use R");
                ComboRHealth = new MenuSlider("teckata.combo.rhealth", "Enemy Health %", 30);
                comboMenu.Add(ComboQ);
                comboMenu.Add(ComboW);
                comboMenu.Add(ComboE);
                comboMenu.Add(ComboEAlways);
                comboMenu.Add(ComboR);
                comboMenu.Add(ComboRHealth);
                
                FullMenu.Add(comboMenu);
            }
            {
                Menu harassMenu = new Menu("teckata.harass", "Harass");
                HarassQ = new MenuBool("teckata.harass.q", "Use Q");
                HarassW = new MenuBool("teckata.harass.w", "Use W");
                HarassE = new MenuBool("teckata.harass.e", "Use E", false);
                harassMenu.Add(HarassQ);
                harassMenu.Add(HarassW);
                harassMenu.Add(HarassE);
                
                FullMenu.Add(harassMenu);
            }
            {
                Menu laneClearMenu = new Menu("teckata.laneclear", "Laneclear");
                LaneClearQ = new MenuBool("teckata.laneclear.q", "Use Q");
                LaneClearW = new MenuBool("teckata.laneclear.w", "Use W");
                LaneClearE = new MenuBool("teckata.laneclear.e", "Use E", false);
                laneClearMenu.Add(LaneClearQ);
                laneClearMenu.Add(LaneClearW);
                laneClearMenu.Add(LaneClearE);
                
                FullMenu.Add(laneClearMenu);
            }
            {
                Menu jungleClearMenu = new Menu("teckata.jungleclear", "Jungleclear");
                JungleClearQ = new MenuBool("teckata.jungleclear.q", "Use Q");
                JungleClearW = new MenuBool("teckata.jungleclear.w", "Use W");
                JungleClearE = new MenuBool("teckata.jungleclear.e", "Use E");
                jungleClearMenu.Add(JungleClearQ);
                jungleClearMenu.Add(JungleClearW);
                jungleClearMenu.Add(JungleClearE);

                FullMenu.Add(jungleClearMenu);
            }
            {
                Menu lastHitMenu = new Menu("teckata.lasthit", "Lasthit");
                LastHitQ = new MenuBool("teckata.lasthit.q", "Use Q");
                LastHitE = new MenuBool("teckata.lasthit.e", "Use E", false);
                lastHitMenu.Add(LastHitQ);
                lastHitMenu.Add(LastHitE);
                
                FullMenu.Add(lastHitMenu);
            }
            {
                Menu killStealMenu = new Menu("teckata.ks", "Killsteal");
                KillSteal = new MenuBool("teckata.ks.use", "Use Killsteal");
                KillStealDisturbR = new MenuBool("teckata.ks.disturbr", "Stop R for Killsteal");
                killStealMenu.Add(KillSteal);
                killStealMenu.Add(KillStealDisturbR);

                FullMenu.Add(killStealMenu);
            }
            {
                Menu drawingsMenu = new Menu("teckata.draw", "Drawings");
                DrawQ = new MenuBool("teckata.drawq", "Draw Q", false);
                DrawE = new MenuBool("teckata.drawe", "Draw E", false);
                DrawR = new MenuBool("teckata.drawr", "Draw R", false);
                DrawDagger = new MenuBool("teckata.drawdagger", "Draw Daggers");
                drawingsMenu.Add(DrawQ);
                drawingsMenu.Add(DrawE);
                drawingsMenu.Add(DrawR);
                FullMenu.Add(drawingsMenu);
            }
            {
                Menu miscMenu = new Menu("teckata.misc", "Misc");
                MiscPreventRCancel = new MenuBool("teckata.misc.preventrcancel", "Prevent R Cancel");
                MiscPreventRCancel.SetToolTip("Will prevent accidental R Cancel within first 0.4 Seconds of R Cast");
                miscMenu.Add(MiscPreventRCancel);

                FullMenu.Add(miscMenu);
            }
            FullMenu.Attach();
        }
    }
}