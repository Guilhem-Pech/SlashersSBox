using System.Collections.Generic;

namespace Sandbox.pawn.pawncontrollers.abilities;

public class AbilityController : EntityComponent<Pawn>
{
	private readonly List<IAbility> m_currentAbilities = new List<IAbility>();


	public void AddAbility(IAbility ability)
	{
		// May need to add a check that a player doesn't already have the ability
		m_currentAbilities.Add( ability );
		ability.OnActivate(Entity);
	}
	
	

	protected override void OnDeactivate()
	{
		foreach (IAbility currentAbility in m_currentAbilities)
		{
			currentAbility.OnDeactivate();
		}
		m_currentAbilities.Clear();
	}
}
