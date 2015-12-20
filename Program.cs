using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using DatJinx;

namespace DatJinx
{
    internal class Program
    {
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        static Item Healthpot;
        public static DamageIndicator Indicator;
        public static Tracker Tracks;
        public static readonly string[] JungleMobsList = { "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab" };
        public static Menu Menu, ComboSettings, HarassSettings, ClearSettings, AutoSettings, DrawMenu, Predictions, Items, Tracker;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Jinx)
            {
                return;
            }
            Teleport.OnTeleport += Teleport_OnTeleport;
            
            Indicator = new DamageIndicator();
            Tracks = new DatJinx.Tracker();
            Healthpot = new Item(2003, 0);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 1450, SkillShotType.Linear, 600, 3300, 75)
            {
                MinimumHitChance = HitChance.Medium,
                AllowedCollisionCount = 0
            };
            E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1);
            R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear, 700, 1500, 140); 
                       

            Menu = MainMenu.AddMenu("Dat Jinx", "DatJinx");

            ComboSettings = Menu.AddSubMenu("Combo Settings", "ComboSettings");
            ComboSettings.Add("useQCombo", new CheckBox("Use Q"));
            ComboSettings.Add("useQAoE", new CheckBox("Use Q AoE"));
            ComboSettings.Add("useQAoECount", new Slider("Q Enemy Count >= ", 3, 1, 5));
            ComboSettings.Add("useWCombo", new CheckBox("Use W"));
            ComboSettings.Add("useECombo", new CheckBox("Use E"));
            ComboSettings.Add("useEDistance", new CheckBox("Use E for Enemy Distance"));
            ComboSettings.Add("EMaxDistance", new Slider("Enemy Distance < ", 200, 100, 900));
            ComboSettings.Add("useRCombo", new CheckBox("Use R"));
            ComboSettings.Add("useRComboRange", new Slider("R Max Range ", 3000, 1000, 4000));

            HarassSettings = Menu.AddSubMenu("Harass Settings", "HarassSettings");
            HarassSettings.Add("useQHarass", new CheckBox("Use Q"));
            HarassSettings.Add("HarassQAoECount", new Slider("Q Enemy Count >= ", 2, 1, 5));
            HarassSettings.Add("useWHarass", new CheckBox("Use W"));
            HarassSettings.Add("useWHarassMana", new Slider("W Mana > %", 20, 0, 100));
            HarassSettings.Add("useEHarass", new CheckBox("Use E"));
            HarassSettings.Add("useEHarassMana", new Slider("E Mana > %", 35, 0, 100));
            HarassSettings.AddSeparator();
            HarassSettings.AddLabel("Auto Harass");
            HarassSettings.Add("autoWHarass", new CheckBox("Auto W for Harass", false));
            HarassSettings.Add("autoWHarassMana", new Slider("W Mana > %", 35, 0, 100));

            ClearSettings = Menu.AddSubMenu("Lane Clear Settings", "FarmSettings");
            ClearSettings.AddLabel("Lane Clear");
            ClearSettings.Add("useQFarm", new CheckBox("Use Q"));
            ClearSettings.Add("disableRocketsWC", new CheckBox("Only Minigun", false));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Last Hit");
            ClearSettings.Add("disableRocketsLH", new CheckBox("Only Minigun"));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Jungle Clear");
            ClearSettings.Add("useQJungle", new CheckBox("Use Q"));
            ClearSettings.Add("useWJungle", new CheckBox("Use W"));
            ClearSettings.Add("useWHarassMana", new Slider("W Mana > ", 20, 0, 100));
            ClearSettings.AddSeparator();
            ClearSettings.Add("RJungleSteal", new CheckBox("Use R Jungle Steal"));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Epics");
            ClearSettings.Add("SRU_Baron", new CheckBox("Baron"));
            ClearSettings.Add("SRU_Dragon", new CheckBox("Dragon"));
            ClearSettings.AddLabel("Buffs");
            ClearSettings.Add("SRU_Blue", new CheckBox("Blue", false));
            ClearSettings.Add("SRU_Red", new CheckBox("Red", false));

            AutoSettings = Menu.AddSubMenu("Misc Settings", "MiscSettings");
            AutoSettings.Add("gapcloser", new CheckBox("Auto E for Gapcloser"));
            AutoSettings.Add("interrupter", new CheckBox("Auto E for Interrupter"));
            AutoSettings.Add("CCE", new CheckBox("Auto E on Enemy CC"));
            AutoSettings.Add("Casting", new CheckBox("Dont use Spell while Enemy Spell Casting",false));

            Items = Menu.AddSubMenu("Item Settings", "ItemSettings");
            Items.Add("useHP", new CheckBox("Use Health Potion"));
            Items.Add("useHPV", new Slider("HP < %", 40, 0, 100));
            Items.AddSeparator();
            Items.Add("useBOTRK", new CheckBox("Use BOTRK"));
            Items.Add("useBotrkMyHP", new Slider("My Health < ", 60, 1, 100));
            Items.Add("useBotrkEnemyHP", new Slider("Enemy Health < ", 60, 1, 100));
            Items.Add("useYoumu", new CheckBox("Use Youmu"));
            Items.Add("useQSS", new CheckBox("Use QSS"));

            Predictions = Menu.AddSubMenu("Prediction Settings", "PredictionSettings");
            var Style = Predictions.Add("style", new Slider("Min Prediction", 1, 0, 2));
            Style.OnValueChange += delegate
            {
                Style.DisplayName = "Min Prediction: " + new[] { "Low", "Medium", "High" }[Style.CurrentValue];
            };
            Style.DisplayName = "Min Prediction: " + new[] { "Low", "Medium", "High" }[Style.CurrentValue];

            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.Add("drawRange", new CheckBox("Draw AA Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawE", new CheckBox("Draw E Range"));
            DrawMenu.AddSeparator();
            DrawMenu.AddLabel("Damage Calculation//Not Work");
            DrawMenu.Add("draw.Damage", new CheckBox("Draw Damage"));
            DrawMenu.Add("draw.Q", new CheckBox("Q Calculate"));
            DrawMenu.Add("draw.W", new CheckBox("W Calculate"));
            DrawMenu.Add("draw.E", new CheckBox("E Calculate"));
            DrawMenu.Add("draw.R", new CheckBox("R Calculate"));
            DrawMenu.AddSeparator();
            DrawMenu.AddLabel("Recall Tracker");
            DrawMenu.Add("draw.Recall", new CheckBox("Chat Print"));

            Tracker = Menu.AddSubMenu("Tracker");
            Tracker.Add("draw.Cooldowns", new CheckBox("Draw Cooldowns"));
            Tracker.Add("draw.Disable", new CheckBox("Disable Draw"));

            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (AutoSettings["interrupter"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.DangerLevel == DangerLevel.High && sender.IsValidTarget(900))
            {
                E.Cast(sender);
            }
        }
        //Recall Tracker Start
        private static string FormatTime(double time)
        {
            var t = TimeSpan.FromSeconds(time);
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private static void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            if (sender.Team == _Player.Team || !DrawMenu["draw.Recall"].Cast<CheckBox>().CurrentValue) return;

            if (args.Status == TeleportStatus.Start)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#00ff66'>started</font> recall.");
            }

            if (args.Status == TeleportStatus.Abort)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#ff0000'>aborted</font> recall.");
            }

            if (args.Status == TeleportStatus.Finish)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#fdff00'>finished</font> recall.");
            }
        }
        //Recall Tracker Finish
        private static void Game_OnTick(EventArgs args)
        {
            var HPpot = Items["useHP"].Cast<CheckBox>().CurrentValue;
            var HPv = Items["useHPv"].Cast<Slider>().CurrentValue;
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                var style = Predictions["style"].Cast<Slider>().CurrentValue;
                switch (style)
                {
                    case 0:
                        ComboLow();
                        break;
                    case 1:
                        ComboMedium();
                        break;
                    case 2:
                        ComboHigh();
                        break;
                    default:
                        ComboMedium();
                        break;
                }
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                WaveClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                //JungleClear();
            }
            Auto();
            KS();
            AutoW();
            if (HPpot && _Player.HealthPercent < HPv)
            {
                if (Item.HasItem(Healthpot.Id) && Item.CanUseItem(Healthpot.Id) && !_Player.HasBuff("RegenerationPotion"))
                {
                    Healthpot.Cast();
                }
            }
        }
        public static void Auto()
        {//AUTO SETTİNGS START
            var useQSS = Items["useQSS"].Cast<CheckBox>().CurrentValue;
            var EonCC = AutoSettings["CCE"].Cast<CheckBox>().CurrentValue;
            if (EonCC)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    if (enemy.Distance(Player.Instance) < E.Range &&
                        (enemy.HasBuffOfType(BuffType.Stun)
                         || enemy.HasBuffOfType(BuffType.Snare)
                         || enemy.HasBuffOfType(BuffType.Suppression)
                         || enemy.HasBuffOfType(BuffType.Fear)
                         || enemy.HasBuffOfType(BuffType.Knockup)))
                    {
                        E.Cast(enemy);
                    }
                }
            }
            if (_Player.HasBuffOfType(BuffType.Fear) || _Player.HasBuffOfType(BuffType.Stun) || _Player.HasBuffOfType(BuffType.Taunt) || _Player.HasBuffOfType(BuffType.Polymorph))
            {
                
                if (useQSS && Item.HasItem(3140) && Item.CanUseItem(3140))
                    Item.UseItem(3140);
            }
        }//AUTO SETTİNGS END
        public static void AutoW()
        {//AUTO W START
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);

            if (HarassSettings["autoWHarass"].Cast<CheckBox>().CurrentValue &&
                wPred.HitChance >= HitChance.Medium && W.IsReady() && targetW.IsValidTarget(W.Range) && _Player.ManaPercent > HarassSettings["autoWHarassMana"].Cast<Slider>().CurrentValue)
            {
                W.Cast(targetW);
            }
        }//AUTO W END
        public static void LastHit()
        {//LASTHİT START
            var menu = ClearSettings["disableRocketsLH"].Cast<CheckBox>().CurrentValue && FishBonesActive;
            if (menu)
            {
                Q.Cast();
            }
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (_Player.Distance(enemy) <= _Player.AttackRange)
                {
                    Harass();
                    Orbwalker.ForcedTarget = enemy;
                    // Regular Q Logic
                    if (FishBonesActive)
                    {
                        if (enemy.Distance(_Player) <= _Player.AttackRange - FishBonesBonus)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (enemy.Distance(_Player) > _Player.AttackRange)
                        {
                            Q.Cast();
                        }
                    }
                    return;
                }
            }
        }//LASTHİT END

        public static void Flee()
        {//Flee START
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var targetE = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);

            if(wPred.HitChance >= HitChance.Medium && W.IsReady() && targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }
            if (E.IsReady() && targetE.IsValidTarget(500))
            {
                E.Cast(targetE);
            }
        }//Flee END

        public static void JungleClear()
        { //Jungle Clear START
            var menu = ClearSettings["useQJungle"].Cast<CheckBox>().CurrentValue;
            var disable = ClearSettings["disableRocketsLH"].Cast<CheckBox>().CurrentValue && FishBonesActive;
            if (Orbwalker.IsAutoAttacking) return;
            if (menu)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters()
                        .Where(
                            a =>
                                a.IsValidTarget(MinigunRange(a) + FishBonesBonus) &&
                                a.Health < _Player.GetAutoAttackDamage(a) * 1.1);

                if (unit != null)
                {
                    if (!FishBonesActive)
                    {
                        Q.Cast();
                    }
                    return;
                }

                if (FishBonesActive)
                {
                    Q.Cast();
                }
            }
            else if (disable)
            {
                Q.Cast();
            }
        } // Jungle Clear END

        public static void WaveClear()
        {//LANE CLEAR START
            var menu = ClearSettings["useQFarm"].Cast<CheckBox>().CurrentValue;
            var disable = ClearSettings["disableRocketsLH"].Cast<CheckBox>().CurrentValue && FishBonesActive;
            if (Orbwalker.IsAutoAttacking) return;

            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if(_Player.Distance(enemy) <= _Player.AttackRange)
                {
                    Harass();
                    Orbwalker.ForcedTarget = enemy;
                    // Regular Q Logic
                    if (FishBonesActive)
                    {
                        if (enemy.Distance(_Player) <= _Player.AttackRange - FishBonesBonus)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (enemy.Distance(_Player) > _Player.AttackRange)
                        {
                            Q.Cast();
                        }
                    }
                    return;
                }
            }

                if (menu)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.IsValidTarget(MinigunRange(a) + FishBonesBonus) &&
                                a.Health < _Player.GetAutoAttackDamage(a)*1.1)
                        .FirstOrDefault(minion => EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                            a => a.Distance(minion) < 150 && a.Health < _Player.GetAutoAttackDamage(a)*1.1) > 1);

                if (unit != null)
                {
                    if (!FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = unit;
                    return;
                }

                if (FishBonesActive)
                {
                    Q.Cast();
                }
            }
            else if (disable)
            {
                Q.Cast();
            }
        }//LANE CLEAR END

        public static void Harass()
        {//HARASS START
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);
            var Wmana = HarassSettings["useWHarassMana"].Cast<Slider>().CurrentValue;
            var Emana = HarassSettings["useEHarassMana"].Cast<Slider>().CurrentValue;

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                // W out of range
                if (HarassSettings["useWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    target.Distance(_Player) > _Player.AttackRange &&
                    targetW.IsValidTarget(W.Range) && _Player.ManaPercent > Wmana)
                {
                    W.Cast(targetW);
                }
            }

            if (target != null)
            {
                var qcount = HarassSettings["HarassQAoECount"].Cast<Slider>().CurrentValue;
                if (HarassSettings["useQHarass"].Cast<CheckBox>().CurrentValue)
                {
                    // Aoe Logic
                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                a => a.IsValidTarget(MinigunRange(a) + FishBonesBonus))
                                .OrderBy(TargetSelector.GetPriority))
                    {
                        if (enemy.CountEnemiesInRange(_Player.AttackRange) >= qcount &&
                            (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 140)) //değiştirildi
                        {
                            if (!FishBonesActive)
                            {
                                Q.Cast();
                            }
                            Orbwalker.ForcedTarget = enemy;
                            return;
                        }
                    }

                    // Regular Q Logic
                    if (FishBonesActive)
                    {
                        if (target.Distance(_Player) <= _Player.AttackRange - FishBonesBonus)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (target.Distance(_Player) > _Player.AttackRange)
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }//HARASS END

        //EVENTS
        public static float FishBonesBonus
        {
            get { return 75f + 25f * Q.Level; }
        }

        public static float MinigunRange(Obj_AI_Base target = null)
        {
            return (590 + (target != null ? target.BoundingRadius : 0));
        }

        public static bool FishBonesActive
        {
            get { return _Player.AttackRange > 525; }
        }

        public const int AoeRadius = 200;

        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (AutoSettings["gapcloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.End.Distance(_Player) < 200)
            {
                E.Cast(e.End);
            }
        }

        public static void KS()
        {// KİLLSTEAL START
            var Distance = ComboSettings["useRComboRange"].Cast<Slider>().CurrentValue;
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);         
            var wPred = W.GetPrediction(targetW);

            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (ComboSettings["useRCombo"].Cast<CheckBox>().CurrentValue && R.IsReady() && enemy.Distance(_Player) <= Distance &&
                RDamage(enemy) >= enemy.Health && !enemy.IsZombie && !enemy.IsDead)
                {
                    R.Cast(enemy);
                    if(DrawMenu["draw.Recall"].Cast<CheckBox>().CurrentValue)
                    {
                        Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + enemy.BaseSkinName + " has <font color='#E238EC'>BOOM!</font>");
                    }                  
                }
            }

            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue &&
                wPred.HitChance >= HitChance.Medium && W.IsReady() && targetW.IsValidTarget(W.Range) &&
                WDamage(targetW) >= targetW.Health)
            {
                W.Cast(targetW);
            }

            
                
        }//KİLLSTEAL END
        public static void ComboLow()
        {//COMBO LOW PREDİCTİON START
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);
            var mtarget = TargetSelector.GetTarget(700, DamageType.Physical);
            var useYoumu = Items["useYoumu"].Cast<CheckBox>().CurrentValue;
            var useMahvolmus = Items["useBOTRK"].Cast<CheckBox>().CurrentValue;
            var useMahvolmusEV = Items["useBotrkEnemyHP"].Cast<Slider>().CurrentValue;
            var useMahvolmusHPV = Items["useBotrkMyHP"].Cast<Slider>().CurrentValue;
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (useMahvolmus && Item.HasItem(3153) && Item.CanUseItem(3153) && Item.HasItem(3144) && Item.CanUseItem(3144) && target.HealthPercent < useMahvolmusEV && _Player.HealthPercent < useMahvolmusHPV)
                Item.UseItem(3153, target);
            Item.UseItem(3144, target);
            
            if (useYoumu && Item.HasItem(3142) && Item.CanUseItem(3142))
                Item.UseItem(3142);


            // E LOGİC
            if (ComboSettings["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Taunt)))
            {
                E.Cast(target);
            }

            if (ComboSettings["useEDistance"].Cast<CheckBox>().CurrentValue && targetW.Distance(_Player) < ComboSettings["EMaxDistance"].Cast<Slider>().CurrentValue)
            {
                E.Cast(targetW);
            }

            // W LOGİC
            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.Distance(_Player) > _Player.AttackRange && wPred.HitChance >= HitChance.Low &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }

            if (ComboSettings["useQAoE"].Cast<CheckBox>().CurrentValue)
            {
                var enemycount = ComboSettings["useQAoECount"].Cast<Slider>().CurrentValue;
                // ALAN(AOE) LOGİC
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                    a => a.IsValidTarget(MinigunRange(a) + FishBonesBonus))                               // alta enemy.distance önüne ekle enemy.NetworkId == target.NetworkId || 
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(_Player.AttackRange) >= enemycount && (enemy.Distance(target) < 140)))
                {
                    if (!FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = enemy;
                    return;
                }
            }

            // Q LOGİC
            if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue && FishBonesActive)
            {
                if (target.Distance(_Player) <= _Player.AttackRange - FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(_Player) > _Player.AttackRange)
                {
                    Q.Cast();
                }
            }

        } //COMBO LOW PREDİCTİON END
        public static void ComboMedium()
        {//COMBO MEDİUM PREDİCTİON START    
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);
            var mtarget = TargetSelector.GetTarget(700, DamageType.Physical);
            var useYoumu = Items["useYoumu"].Cast<CheckBox>().CurrentValue;
            var useMahvolmus = Items["useBOTRK"].Cast<CheckBox>().CurrentValue;
            var useMahvolmusEV = Items["useBotrkEnemyHP"].Cast<Slider>().CurrentValue;
            var useMahvolmusHPV = Items["useBotrkMyHP"].Cast<Slider>().CurrentValue;

            Orbwalker.ForcedTarget = null;


            if (Orbwalker.IsAutoAttacking) return;

            if (useMahvolmus && Item.HasItem(3153) && Item.CanUseItem(3153) && Item.HasItem(3144) && Item.CanUseItem(3144) && target.HealthPercent < useMahvolmusEV && _Player.HealthPercent < useMahvolmusHPV)
                Item.UseItem(3153, target);
            Item.UseItem(3144, target);

            if (useYoumu && Item.HasItem(3142) && Item.CanUseItem(3142))
                Item.UseItem(3142);

            // E LOGİC

            if (ComboSettings["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) ||  target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Taunt)))
            {
                E.Cast(target);
            }

            if (ComboSettings["useEDistance"].Cast<CheckBox>().CurrentValue && targetW.Distance(_Player) < ComboSettings["EMaxDistance"].Cast<Slider>().CurrentValue)
            {
                E.Cast(targetW);
            }

            // W LOGİC
            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.Distance(_Player) > _Player.AttackRange && wPred.HitChance >= HitChance.Medium &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }

            if (ComboSettings["useQAoE"].Cast<CheckBox>().CurrentValue)
            {
                var enemycount = ComboSettings["useQAoECount"].Cast<Slider>().CurrentValue;
                // ALAN(AOE) LOGİC
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                    a => a.IsValidTarget(MinigunRange(a) + FishBonesBonus)) //                                        // alta enemy.distance önüne ekle enemy.NetworkId == target.NetworkId || 
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(_Player.AttackRange) >= enemycount && (enemy.Distance(target) < 140)))
                {
                    if (!FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = enemy;
                    return;
                }
            }

            // Q LOGİC
            if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue && FishBonesActive)
            {
                if (target.Distance(_Player) <= _Player.AttackRange - FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else if(ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(_Player) > _Player.AttackRange)
                {
                    Q.Cast();
                }
            }

    } //COMBO MEDİUM PREDİCTİON END
        public static void ComboHigh()
        {//COMBO HİGH PREDİCTİON START
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);
            var mtarget = TargetSelector.GetTarget(700, DamageType.Physical);
            var useYoumu = Items["useYoumu"].Cast<CheckBox>().CurrentValue;
            var useMahvolmus = Items["useBOTRK"].Cast<CheckBox>().CurrentValue;
            var useMahvolmusEV = Items["useBotrkEnemyHP"].Cast<Slider>().CurrentValue;
            var useMahvolmusHPV = Items["useBotrkMyHP"].Cast<Slider>().CurrentValue;

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;


            if (useMahvolmus && Item.HasItem(3153) && Item.CanUseItem(3153) && Item.HasItem(3144) && Item.CanUseItem(3144) && target.HealthPercent < useMahvolmusEV && _Player.HealthPercent < useMahvolmusHPV)
                Item.UseItem(3153, target);
            Item.UseItem(3144, target);

            if (useYoumu && Item.HasItem(3142) && Item.CanUseItem(3142))
                Item.UseItem(3142);

            // E LOGİC

            if (ComboSettings["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Taunt)))
            {
                E.Cast(target);
            }

            if (ComboSettings["useEDistance"].Cast<CheckBox>().CurrentValue && targetW.Distance(_Player) < ComboSettings["EMaxDistance"].Cast<Slider>().CurrentValue)
            {
                E.Cast(targetW);
            }

            // W LOGİC
            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.Distance(_Player) > _Player.AttackRange && wPred.HitChance >= HitChance.High &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }

            if (ComboSettings["useQAoE"].Cast<CheckBox>().CurrentValue)
            {
                var enemycount = ComboSettings["useQAoECount"].Cast<Slider>().CurrentValue;
                // ALAN(AOE) LOGİC
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                    a => a.IsValidTarget(MinigunRange(a) + FishBonesBonus)) //                                                  // alta enemy.distance önüne ekle enemy.NetworkId == target.NetworkId || 
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(_Player.AttackRange) >= enemycount && (enemy.Distance(target) < 140)))
                {
                    if (!FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = enemy;
                    return;
                }
            }

            // Q LOGİC
            if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue && FishBonesActive)
            {
                if (target.Distance(_Player) <= _Player.AttackRange - FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else if (ComboSettings["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(_Player) > _Player.AttackRange)
                {
                    Q.Cast();
                }
            }

        } //COMBO HİGH PREDİCTİON END
        //DAMAGE HESAP
        public static int WDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new int[] { 10, 60, 110, 160, 210 }[W.Level - 1] +
                     1.4 * (_Player.TotalAttackDamage));
        }

        public static int EDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new double[] { 80, 135, 190, 245, 300 }[E.Level - 1]
                                    + 1 * _Player.FlatMagicDamageMod);
        }

        public static float RDamage(Obj_AI_Base target)
        {
            
            if (!DatJinx.Program.R.IsLearned) return 0;
             var level = DatJinx.Program.R.Level - 1;

                if (target.Distance(_Player) < 1350)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                        (float)
                            (new double[] { 25, 35, 45 }[level] +
                             new double[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                             0.1 * _Player.TotalAttackDamage));
                }

                return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)
                        (new double[] { 250, 350, 450 }[level] +
                         new double[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                         1 * _Player.TotalAttackDamage));

            
        }

        private static void JungleSteal()
        {
            var rRange = ComboSettings["useRComboRange"].Cast<Slider>().CurrentValue;
                    var jungleMob =
                        EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(
                            u =>
                            u.IsVisible && JungleMobsList.Contains(u.BaseSkinName)
                            && RDamage(u) >= u.Health);

                    if (jungleMob == null)
                    {
                        return;
                    }

                    if (!ClearSettings[jungleMob.BaseSkinName].Cast<CheckBox>().CurrentValue)
                    {
                        return;
                    }

                    var enemy = EntityManager.Heroes.Enemies.Where(t => t.Distance(jungleMob) <= 200).OrderByDescending(t => t.Distance(jungleMob));

                    if (enemy.Any())
                    {
                        foreach (var target in enemy.Where(target => Player.Instance.Distance(target) < rRange))
                        {
                            if (target.Distance(jungleMob) <= 200)
                            {
                                R.Cast(target);
                            }
                        }
                    }                                           
        }

        //DRAWİNGS
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawRange"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.DarkSeaGreen, !FishBonesActive ? FishBonesBonus + MinigunRange() - _Player.BoundingRadius / 2 : MinigunRange() - _Player.BoundingRadius / 2, _Player.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.Gray : Color.Red, W.Range, _Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? Color.Gray : Color.Red, E.Range, _Player.Position);
            }
        }


    }


}