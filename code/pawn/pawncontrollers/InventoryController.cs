using System;
using System.Collections.Generic;
using Sandbox.Utils;
using Sandbox.weapon;

namespace Sandbox.pawn.PawnControllers;

/// <summary>
/// The player's inventory holds the player's weapons, and holds the player's current weapon.
/// It also drives functionality such as weapon switching.
/// </summary>
public partial class InventoryController : EntityComponent<Pawn>, ISingletonComponent, IComponentSimulable, IEventListener
{
	private IList<weapon.Weapon> m_ownedWeapons = new List<weapon.Weapon>();
	
	[Net, Predicted] public weapon.Weapon? ActiveWeapon { get; private set; }
	private IDictionary<AmmoType, int> m_currentAmmunitionAvailable = new Dictionary<AmmoType, int>(); 
	
	public void Simulate( IClient client )
	{
		ActiveWeapon?.Simulate( client );
	}
	
	public int AmmoCount( AmmoType configAmmoType )
	{
		m_currentAmmunitionAvailable.TryGetValue( configAmmoType, out var value );
		return value;
	}
	public void GiveAmmo( AmmoType configAmmoType, int remainingAmmo )
	{
		if(m_currentAmmunitionAvailable.ContainsKey(configAmmoType)) 
		{
			m_currentAmmunitionAvailable[configAmmoType] += remainingAmmo;
		}
		else 
		{
			m_currentAmmunitionAvailable[configAmmoType] = remainingAmmo;
		}
	}
	
	public int TakeAmmo(AmmoType configAmmoType, int clipSize)
	{
		if (!m_currentAmmunitionAvailable.TryGetValue(configAmmoType, out var current))
		{
			return 0;
		}

		int ammoToReturn = Math.Min(current, clipSize);
		m_currentAmmunitionAvailable[configAmmoType] -= ammoToReturn;

		return ammoToReturn;
	}
}
