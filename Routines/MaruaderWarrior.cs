using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using TreeSharp;

using Action = TreeSharp.Action;
namespace Kupo.Rotations
{
    public class MaruaderWarrior : KupoRoutine
    {
		private int _pullRange = 5;
        public override int PullRange
        {
            get { return this._pullRange; }
        }

        public override ClassJobType[] Class
        {
            get { return new ClassJobType[]{ClassJobType.Marauder, ClassJobType.Warrior }; }
        }

        protected override Composite CreatePull()
        {
			//Check to see if we have Tomahawk if so change our pull logic
			if (Actionmanager.HasSpell("Tomahawk")) {
				this._pullRange = 15;
				return new PrioritySelector(r => Actionmanager.InSpellInRangeLOS("Tomahawk", Core.Target),
					new Decorator(
						r => Actionmanager.InSpellInRangeLOS("Tomahawk", Core.Target) == SpellRangeCheck.ErrorNotInRange,
						new Action(r => Navigator.MoveTo(Core.Target.Location))),

					//Checking to see if we're not facing the target -- If not, face it
					new Decorator(
						r => Actionmanager.InSpellInRangeLOS("Tomahawk", Core.Target) == SpellRangeCheck.ErrorNotInFront,
						new Action(r => Core.Target.Face())),

					Cast("Tomahawk", r => Actionmanager.InSpellInRangeLOS("Tomahawk", Core.Target) == SpellRangeCheck.Success && Actionmanager.LastSpell.Name != "Tomahawk")
					//We've casted Tomahawk, now meet it -- however the bahvior is funny with this right now so going to leave it out
					//	I figure that it's better to cast tomahawk and let it come to me than look "botlike" and run circles around the mob trying to meet it
					/*new Decorator(
						r => Actionmanager.LastSpell.Name == "Tomahawk",
						new Action(r => Navigator.MoveTo(Core.Target.Location)))*/
				);
			}
            return new PrioritySelector( r=> Actionmanager.InSpellInRangeLOS("Heavy Swing", Core.Target),
                new Decorator(
					r => (r as SpellRangeCheck?) == SpellRangeCheck.ErrorNotInRange,
					new Action(r => Navigator.MoveTo(Core.Target.Location))),
					Cast("Heavy Swing", r => (r as SpellRangeCheck?) == SpellRangeCheck.Success || (r as SpellRangeCheck?) == SpellRangeCheck.ErrorNotInFront)
            );
        }

		protected override Composite CreateHeal()
		{
			return new PrioritySelector(
				Apply("Foresight", r => Core.Player.HealthPercent <= 80 && Actionmanager.HasSpell("Foresight")),
				Apply("Bloodbath", r => Core.Player.HealthPercent <= 80 && Actionmanager.HasSpell("Bloodbath")),
				Cast("Inner Beast", r => Core.Player.HealthPercent <= 60 && Core.Player.HasAura("Infuriated") && Actionmanager.HasSpell("Inner Beast")),
				Cast("Convalescence", r => Core.Player.HealthPercent <= 50 && Actionmanager.HasSpell("Convalescence")),
				Apply("Thrill of Battle", r => Core.Player.HealthPercent <= 30)
			);
		}

        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
                Apply("Fracture"),
				Cast("Storm's Eye", r => Actionmanager.LastSpell.Name == "Maim" && Actionmanager.HasSpell("Storm's Eye")),
				Cast("Butcher's Block", r => Actionmanager.LastSpell.Name == "Skull Sunder" && Actionmanager.HasSpell("Butcher's Block")),
				Cast("Skull Sunder", r => Actionmanager.LastSpell.Name == "Heavy Swing" && (!Actionmanager.HasSpell("Maim") || Core.Player.HasAura("Maim"))),
				Cast("Maim", r => Actionmanager.LastSpell.Name == "Heavy Swing" && Actionmanager.HasSpell("Maim")),
				Cast("Heavy Swing", r => true)
            );
        }

    }
}
