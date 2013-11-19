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
    class ConjurerWhiteMage : KupoRoutine
    {
		private bool needHeals = false;
        public override ClassJobType[] Class
        {
			get { return new[] { ClassJobType.Conjurer, ClassJobType.WhiteMage, }; }
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
					r => Actionmanager.InSpellInRangeLOS("Stone", Core.Target) == SpellRangeCheck.ErrorNotInRange,
					new Action(r => Navigator.MoveTo(Core.Target.Location))),

				//Checking to see if we're not facing the target -- If not, face it
                new Decorator(
					r => Actionmanager.InSpellInRangeLOS("Stone", Core.Target) == SpellRangeCheck.ErrorNotInFront,
					new Action(r => Core.Target.Face())),

				//We'll open with a stone for heavy
                Cast("Stone", r => Actionmanager.InSpellInRangeLOS("Stone", Core.Target) == SpellRangeCheck.Success)
            );
        }
		protected override Composite CreatePreCombatBuffs()
        {
            return new PrioritySelector(
                Apply("Protect", r => !Core.Player.HasAura("Protect"), r=> Core.Player)
            );
        }
        protected override Composite CreateHeal()
        {
			this.needHeals = Core.Player.HealthPercent <= 40;
			return new PrioritySelector(
				//Check to see if we have cleric stance up or not -- If so, remove it for better heals
				Apply("Cleric Stance", r => Core.Player.HasAura("Cleric Stance") && this.needHeals, r => Core.Player),
				//If we have a free Cure II and we have Cure II use it!
				Cast("Cure II", r => Core.Player.HasAura("Freecure"), r => Core.Player),
				Cast("Cure", r => this.needHeals, r => Core.Player)
            );
          
        }
        protected override Composite CreateCombat()
        {
            return new PrioritySelector(
				//Check to see if we have cleric stance up or not -- Gotta get them deepz
				Cast("Cleric Stance", r => !Core.Player.HasAura("Cleric Stance"), r => Core.Player),

				//Check to see if we need to get mana back
				Cast("Shroud of Saints", r => (Core.Player.MaxMana - Core.Player.Mana > 1200), r => Core.Player),
                
				//Get our DoTs up and going
					//Get the Aero I/II dot up and going
					Apply("Aero II"),
					Apply("Aero", r => Core.Player.ClassLevel < 46),

					//Get the thunder dot up and going
					Apply("Thunder"),
				
				//Use the push back if we can
				Cast("Fluid Aura", r => Core.Target.Distance2D <= 15f),

				//Bread and butter Stone I/II spam
				Cast("Stone", r => Core.Player.ClassLevel < 22),
				Cast("Stone II")
            );
        }
    }
}