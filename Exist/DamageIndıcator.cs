using System;

using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

using SharpDX;

using Color = System.Drawing.Color;


namespace DatJinx
{
    public class DamageIndicator
    {
        private const float BarLength = 104;
        private const float XOffset = 0;
        private const float YOffset = 11;
        public float CheckDistance = 1200;

        public DamageIndicator()
        {
            Drawing.OnEndScene += Drawing_OnDraw;
        }

        public static class DamageLibrary
        {
            public static float CalculateDamage(Obj_AI_Base target, bool Q, bool W, bool E, bool R)
            {
                var totaldamage = 0f;

                if (Q && Program.Q.IsReady())
                {
                    totaldamage = totaldamage + QDamage(target);
                }

                if (W && Program.W.IsReady())
                {
                    totaldamage = totaldamage + WDamage(target);
                }

                if (E && Program.E.IsReady())
                {
                    totaldamage = totaldamage + EDamage(target);
                }

                if (R && Program.R.IsReady())
                {
                    totaldamage = totaldamage + RDamage(target);
                }

                return totaldamage;
            }

            private static float QDamage(Obj_AI_Base target)
            {
                return Player.Instance.GetAutoAttackDamage(target);
            }

            private static float WDamage(Obj_AI_Base target)
            {
                return Player.Instance.CalculateDamageOnUnit(
                    target,
                    DamageType.Physical,
                    new[] { 0, 10, 60, 110, 160, 210 }[Program.W.Level])
                       + (Player.Instance.TotalAttackDamage * 1.4f);
            }

            private static float EDamage(Obj_AI_Base target)
            {
                return Player.Instance.CalculateDamageOnUnit(
                    target,
                    DamageType.Magical,
                    new[] { 0, 80, 135, 190, 245, 300 }[Program.E.Level] + (Player.Instance.TotalMagicalDamage));
            }

            private static float RDamage(Obj_AI_Base target)
            {
                if (!Program.R.IsLearned) return 0;
                var level = Program.R.Level - 1;

                if (target.Distance(Player.Instance) < 1350)
                {
                    return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                        (float)
                            (new double[] { 25, 35, 45 }[level] +
                             new double[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                             0.1 * Player.Instance.TotalAttackDamage));
                }

                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)
                        (new double[] { 250, 350, 450 }[level] +
                         new double[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                         1 * Player.Instance.TotalAttackDamage));
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program.DrawMenu["draw.Damage"].Cast<CheckBox>().CurrentValue) return;

            foreach (var aiHeroClient in EntityManager.Heroes.Enemies)
            {
                if (!aiHeroClient.IsHPBarRendered) continue;

                var pos = new Vector2(
                    aiHeroClient.HPBarPosition.X + XOffset,
                    aiHeroClient.HPBarPosition.Y + YOffset);

                var fullbar = (BarLength) * (aiHeroClient.HealthPercent / 100);

                var drawQ = Program.DrawMenu["draw.Q"].Cast<CheckBox>().CurrentValue;

                var drawW = Program.DrawMenu["draw.W"].Cast<CheckBox>().CurrentValue;

                var drawE = Program.DrawMenu["draw.E"].Cast<CheckBox>().CurrentValue;

                var drawR = Program.DrawMenu["draw.R"].Cast<CheckBox>().CurrentValue;

                var damage = (BarLength)
                             * ((DamageLibrary.CalculateDamage(aiHeroClient, drawQ, drawW, drawE, drawR)
                                 / aiHeroClient.MaxHealth) > 1
                                    ? 1
                                    : (DamageLibrary.CalculateDamage(
                                        aiHeroClient,
                                        drawQ,
                                        drawW,
                                        drawE,
                                        drawR) / aiHeroClient.MaxHealth));

                Line.DrawLine(
                    Color.FromArgb(100, Color.Black),
                    9f,
                    new Vector2(pos.X, pos.Y),
                    new Vector2(pos.X + (damage > fullbar ? fullbar : damage), pos.Y));

                Line.DrawLine(
                    Color.Black,
                    3,
                    new Vector2(pos.X + (damage > fullbar ? fullbar : damage), pos.Y),
                    new Vector2(pos.X + (damage > fullbar ? fullbar : damage), pos.Y));
            }
        }
    }
}
