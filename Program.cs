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

namespace DatJinx
{
    internal class Program
    {
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }


        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;        

        public static Menu Menu, ComboSettings, HarassSettings, ClearSettings, AutoSettings, DrawMenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Jinx)
            {
                return;
            }
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
            ComboSettings.AddSeparator();
            ComboSettings.Add("useQCombo", new CheckBox("Use Q"));
            ComboSettings.Add("useQAoE", new CheckBox("Use Q AoE"));
            ComboSettings.Add("useQAoECount", new Slider("Enemy Count >= ", 2, 1, 5));
            ComboSettings.Add("useWCombo", new CheckBox("Use W"));
            ComboSettings.Add("useECombo", new CheckBox("Use E"));
            ComboSettings.Add("useRCombo", new CheckBox("Use R"));
            ComboSettings.Add("useRComboRange", new Slider("Range < ", 2000, 0, 3000));

            HarassSettings = Menu.AddSubMenu("Harass Settings", "HarassSettings");
            HarassSettings.AddSeparator();
            HarassSettings.Add("useQHarass", new CheckBox("Use Q"));
            HarassSettings.Add("useQHarassMana", new Slider("Mana > ", 35, 0, 100));
            HarassSettings.Add("useWHarass", new CheckBox("Use W"));
            HarassSettings.Add("useWHarassMana", new Slider("Mana > ", 20, 0, 100));
            HarassSettings.Add("useEHarass", new CheckBox("Use E"));
            HarassSettings.Add("useEHarassMana", new Slider("Mana > ", 35, 0, 100));

            ClearSettings = Menu.AddSubMenu("Lane Clear Settings", "FarmSettings");
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Lane Clear");
            ClearSettings.Add("useQFarm", new CheckBox("Use Q"));
            ClearSettings.Add("disableRocketsWC", new CheckBox("Only Minigun", false));
            ClearSettings.AddSeparator();
            ClearSettings.AddLabel("Last Hit");
            ClearSettings.Add("disableRocketsLH", new CheckBox("Only Minigun"));

            AutoSettings = Menu.AddSubMenu("Auto Settings", "AutoSettings");
            AutoSettings.AddSeparator();
            AutoSettings.Add("gapcloser", new CheckBox("Gapcloser E"));
            AutoSettings.Add("interrupter", new CheckBox("Interrupter E"));
            AutoSettings.Add("CCE", new CheckBox("Auto E on CC"));

            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.Add("drawRange", new CheckBox("Draw AA Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawE", new CheckBox("Draw E Range"));
            DrawMenu.AddSeparator();
            DrawMenu.AddLabel("Damage Calculation");
            DrawMenu.Add("draw.Damage", new CheckBox("Draw Damage"));
            DrawMenu.Add("draw.Q", new CheckBox("Q Calculate"));
            DrawMenu.Add("draw.W", new CheckBox("W Calculate"));
            DrawMenu.Add("draw.E", new CheckBox("E Calculate"));
            DrawMenu.Add("draw.R", new CheckBox("R Calculate"));

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

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
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
            Auto();           
        }
        public static void Auto()
        {
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
        }
        public static void LastHit()
        {
            var menu = ClearSettings["disableRocketsLH"].Cast<CheckBox>().CurrentValue && FishBonesActive;
            if (menu)
            {
                Q.Cast();
            }
        }

        public static void WaveClear()
        {
            var menu = ClearSettings["useQFarm"].Cast<CheckBox>().CurrentValue;
            var disable = ClearSettings["disableRocketsLH"].Cast<CheckBox>().CurrentValue && FishBonesActive;
            if (Orbwalker.IsAutoAttacking) return;
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
        }

        public static void Harass()
        {
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                // W out of range
                if (HarassSettings["useWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    target.Distance(_Player) > _Player.AttackRange &&
                    targetW.IsValidTarget(W.Range))
                {
                    W.Cast(targetW);
                }
            }

            if (target != null)
            {

                if (HarassSettings["useQHarass"].Cast<CheckBox>().CurrentValue)
                {
                    // Aoe Logic
                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                a => a.IsValidTarget(MinigunRange(a) + FishBonesBonus))
                                .OrderBy(TargetSelector.GetPriority))
                    {
                        if (enemy.CountEnemiesInRange(150) > 1 &&
                            (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150))
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
        }

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
        {
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);
            // KİLLSTEAL
            var wPred = W.GetPrediction(targetW);
            var rPred = R.GetPrediction(rtarget);

            if (ComboSettings["useWCombo"].Cast<CheckBox>().CurrentValue &&
                wPred.HitChance >= HitChance.Medium && W.IsReady() && targetW.IsValidTarget(W.Range) &&
                WDamage(target) >= rtarget.Health)
            {
                W.Cast(targetW);
            }
            if (ComboSettings["useRCombo"].Cast<CheckBox>().CurrentValue &&
                rPred.HitChance >= HitChance.Medium && R.IsReady() && rtarget.IsValidTarget(R.Range) &&
                DamageLibrary.CalculateDamage(target, false, false, false, true) >= rtarget.Health)
            {
                R.Cast(rtarget);
            }
        }
        public static void Combo()
        {           
            var targetW = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget((!FishBonesActive ? _Player.AttackRange + FishBonesBonus : _Player.AttackRange) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);
            var wPred = W.GetPrediction(targetW);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            // E LOGİC

            if (ComboSettings["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) ||  target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Taunt)))
            {
                E.Cast(target);
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
                    a => a.IsValidTarget(MinigunRange(a) + FishBonesBonus))
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(200) >= enemycount && (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150)))
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

    } //COMBO BİTİŞ
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

        public static int RDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new double[] { 250, 350, 450 }[R.Level - 1]
                                    + 0.5 * _Player.TotalAttackDamage);
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
                Circle.Draw(W.IsReady() ? Color.HotPink : Color.Red, W.Range, _Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? Color.HotPink : Color.Red, E.Range, _Player.Position);
            }
        }


    }


}