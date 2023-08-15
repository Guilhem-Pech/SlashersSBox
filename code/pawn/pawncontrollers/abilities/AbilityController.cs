using System;
using System.Collections.Generic;
using Sandbox.Utils;

namespace Sandbox.pawn.pawncontrollers.abilities;

public class AbilityController : EntityComponent<Pawn>, IComponentSimulable
{
	private readonly Dictionary<string, IAbility> m_currentAbilities = new();

	public void AddAbility( IAbility ability )
	{
		var type = ability.GetType().ToString(); // Need to convert it to be networkable
		if ( m_currentAbilities.TryAdd( type, ability ))
		{
			ability.OnActivate( Entity );
		}
	}

	protected override void OnDeactivate()
	{
		foreach (IAbility ability in m_currentAbilities.Values)
		{
			ability.OnDeactivate();
		}
		m_currentAbilities.Clear();
	}

	public void Simulate( IClient client )
	{
		foreach (IAbility ability in m_currentAbilities.Values)
		{
			if ( ability is not ISimulable simulableAbility )
				continue;
			simulableAbility.Simulate( client );
		}
	}
}
