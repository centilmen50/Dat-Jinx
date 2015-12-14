using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace DatJinx
{
    //NOT WORK
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

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!DatJinx.Program.DrawMenu["draw.Damage"].Cast<CheckBox>().CurrentValue) return;

            foreach (var aiHeroClient in EntityManager.Heroes.Enemies)
            {
                if (!aiHeroClient.IsHPBarRendered) continue;

                var pos = new Vector2(
                    aiHeroClient.HPBarPosition.X + XOffset,
                    aiHeroClient.HPBarPosition.Y + YOffset);

                var fullbar = (BarLength) * (aiHeroClient.HealthPercent / 100);

                var drawQ = DatJinx.Program.DrawMenu["draw.Q"].Cast<CheckBox>().CurrentValue;

                var drawW = DatJinx.Program.DrawMenu["draw.W"].Cast<CheckBox>().CurrentValue;

                var drawE = DatJinx.Program.DrawMenu["draw.E"].Cast<CheckBox>().CurrentValue;

                var drawR = DatJinx.Program.DrawMenu["draw.R"].Cast<CheckBox>().CurrentValue;

                var damage = (BarLength)
                             * ((RDamageHesap.CalculateDamage(aiHeroClient, drawR)
                                 / aiHeroClient.MaxHealth) > 1
                                    ? 1
                                    : (RDamageHesap.CalculateDamage(
                                        aiHeroClient,                                     
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
