using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using Kupo;
using TreeSharp;
using Action = TreeSharp.Action;

namespace Kupo.Rotations
{
	class ThaumaturgeBlackMage : KupoRoutine
	{
		public override ClassJobType[] Class
		{
			get { return new[] { ClassJobType.Thaumaturge, ClassJobType.BlackMage }; }
		}
		public override int PullRange
		{
			get { return 20; }
		}

		protected override Composite CreatePull()
		{
			return new PrioritySelector(
				//Checking to see if we're in range -- If not, move to the location
				new Decorator(
					r => Actionmanager.InSpellInRangeLOS("Blizzard", Core.Target) == SpellRangeCheck.ErrorNotInRange,
					new Action(r => Navigator.MoveTo(Core.Target.Location))),

				//Checking to see if we're not facing the target -- If not, face it
				new Decorator(
					r => Actionmanager.InSpellInRangeLOS("Blizzard", Core.Target) == SpellRangeCheck.ErrorNotInFront,
					new Action(r => Core.Target.Face())),

				Cast("Blizzard", r => Actionmanager.InSpellInRangeLOS("Blizzard", Core.Target) == SpellRangeCheck.Success && Core.Player.ClassLevel < 6),
				Cast("Thunder", r => Actionmanager.InSpellInRangeLOS("Thunder", Core.Target) == SpellRangeCheck.Success && Core.Player.ClassLevel < 22),
				Cast("Thunder II", r => Actionmanager.InSpellInRangeLOS("Thunder II", Core.Target) == SpellRangeCheck.Success && Core.Player.ClassLevel >= 22)
			);
		}
		protected override Composite CreateCombatBuffs()
		{
			return new PrioritySelector(
				//Dem deepz
				Apply("Raging Strikes"),
				Apply("Swiftcast")
			);
		}
		protected override Composite CreateHeal()
		{
			return new PrioritySelector(
				Apply("Manaward", r => Core.Player.HealthPercent <= 80),
				Apply("Manawall", r => Core.Player.HealthPercent <= 80),
				Cast("Physick", r => Core.Player.HealthPercent <= 40, r => Core.Player)
			);
		}
		protected override Composite CreateCombat()
		{
			return new PrioritySelector(
				//Need to check for insta procs first and foremost
				Cast("Thunder III", r => Core.Player.HasAura("Thundercloud")),
				Cast("Fire III", r => Core.Player.HasAura("Firestarter")),
				
				//If we're low on mana we need to make sure we get it back
				Cast("Blizzard III", r => Core.Player.Mana < 638 && Core.Player.ClassLevel >= 38),
				Cast("Blizzard", r => Core.Player.ManaPercent <= 10 && Core.Player.ClassLevel < 38),
				Cast("Convert", r => Core.Player.Mana < 79 && Core.Player.ClassLevel >= 30), //79 Mana is how much Blizzard III is with AstralFire ... don't want to be stuck with no mana

				Apply("Thunder II", r => Core.Player.ClassLevel >= 22 && !Core.Target.HasAura("Thunder")),
				Apply("Thunder", r => Core.Player.ClassLevel < 22),

				Cast("Fire III", r => Core.Player.ClassLevel >= 34 && !Core.Player.HasAura("Astral Fire III") && Core.Player.Mana > 638),
				//Bread and butter Fire spam
				Cast("Fire")
			);
		}
	}
}