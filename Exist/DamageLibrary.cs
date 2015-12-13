using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace DatJinx
{
    public static class DamageLibrary
    {
        public static float CalculateDamage(Obj_AI_Base target, bool Q, bool W, bool E, bool R)
        {
            var totaldamage = 0f;

            if (Q && DatJinx.Program.Q.IsReady())
            {
                totaldamage = totaldamage + QDamage(target);
            }

            if (W && DatJinx.Program.W.IsReady())
            {
                totaldamage = totaldamage + WDamage(target);
            }

            if (E && DatJinx.Program.E.IsReady())
            {
                totaldamage = totaldamage + EDamage(target);
            }

            if (R && DatJinx.Program.R.IsReady())
            {
                totaldamage = totaldamage + RDamage(target);
            }

            return totaldamage;
        }

        private static float QDamage(Obj_AI_Base target)
        {
            return Program._Player.GetAutoAttackDamage(target);
        }

        private static float WDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(
                target,
                DamageType.Physical,
                new[] { 0, 10, 60, 110, 160, 210 }[DatJinx.Program.W.Level])
                   + (Program._Player.TotalAttackDamage * 1.4f);
        }

        private static float EDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(
                target,
                DamageType.Magical,
                new[] { 0, 80, 135, 190, 245, 300 }[DatJinx.Program.E.Level] + (Program._Player.TotalMagicalDamage));
        }

        private static float RDamage(Obj_AI_Base target)
        {
            if (!DatJinx.Program.R.IsLearned) return 0;
            var level = DatJinx.Program.R.Level - 1;

            if (target.Distance(Program._Player) < 1350)
            {
                return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)
                        (new double[] { 25, 35, 45 }[level] +
                         new double[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                         0.1 * Program._Player.TotalAttackDamage));
            }

            return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)
                    (new double[] { 250, 350, 450 }[level] +
                     new double[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                     1 * Program._Player.TotalAttackDamage));
        }
    }
}
        

