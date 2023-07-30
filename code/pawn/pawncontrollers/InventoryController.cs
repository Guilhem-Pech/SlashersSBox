using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Utils;
using Sandbox.weapon;

namespace Sandbox.pawn.PawnControllers;

/// <summary>
/// The player's inventory holds the player's weapons, and holds the player's current weapon.
/// It also drives functionality such as weapon switching.
/// </summary>
public partial class InventoryController : EntityComponent<Pawn>, ISingletonComponent, IComponentSimulable, IEventListener
{
	private IList<Weapon> m_ownedWeapons = new List<Weapon>();
	
	[Net, Predicted] public Weapon? ActiveWeapon { get; private set; }
	public Weapon? LastActiveWeapon { get; private set; }

	[Net, Predicted]
	private List<int> CurrentAmmunitionAvailable { get; set; } = Enumerable.Repeat(0, Enum.GetNames<AmmoType>().Length).ToList(); //Workaround the fact we can't replicate dictionary 

	protected override void OnActivate()
	{
		Entity.EventDispatcher.RegisterEvent<EventOnRespawn>( this, OnPlayerRespawn );
	}
	
	protected override void OnDeactivate()
	{
		Entity.EventDispatcher.UnregisterAllEvents( this ); 
	}
	private void OnPlayerRespawn( EventOnRespawn obj )
	{
		// TODO This is just for test, we'll see later how to handle that
		Add( new Pistol(), true );
	}
	
	public bool Add( Weapon weapon, bool makeActive = false ) 
	{
		Game.AssertServer();

		if ( weapon.IsValid() && IsCarryingType( weapon.GetType() ) )
		{
			var ammo = weapon.AmmoClip;
			var ammoType = weapon.Config.AmmoType;

			if ( ammo > 0 )
			{
				GiveAmmo( ammoType, ammo );
			}

			weapon.Delete();
			return false;
		}

		if ( weapon.Owner.IsValid() )
			return false;

		if ( !CanAdd( weapon ) )
			return false;

		if ( !weapon.IsValid() )
			return false;

		if ( !weapon.CanCarry( Entity ) )
			return false;
		
		m_ownedWeapons.Add( weapon );
		weapon.Parent = Entity;
		weapon.OnCarryStart( Entity );

		if ( makeActive )
		{
			SetActive( weapon );
		}

		return true;
	}
	
	public bool IsCarryingType( Type t )
	{
		return m_ownedWeapons.Any( x => x.GetType() == t );
	}
	
	public bool CanAdd( Weapon? entity )
	{
		return entity != null && entity.CanCarry( Entity );
	}
	
	public virtual bool SetActive( Weapon entity )
	{
		if ( ActiveWeapon == entity ) return false;
		if ( !m_ownedWeapons.Contains( entity ) ) return false;

		LastActiveWeapon = ActiveWeapon;
		LastActiveWeapon?.ActiveEnd( Entity, LastActiveWeapon.Owner != Entity );
		ActiveWeapon = entity;
		ActiveWeapon.ActiveStart(Entity);

		SetActiveClient( To.Everyone );
		
		return true;
	}

	[ClientRpc]
	public void SetActiveClient()
	{
		ActiveWeapon.ActiveStart(Entity);
	}

	public void Simulate( IClient client )
	{
		ActiveWeapon?.Simulate( client );
	}
	
	public int AmmoCount( AmmoType configAmmoType )
	{
		if ( (int)configAmmoType >= 0 && (int) configAmmoType < CurrentAmmunitionAvailable.Count )
		{
			return CurrentAmmunitionAvailable[(int)configAmmoType];
		}
		return 0;
	}
	
	public void GiveAmmo( AmmoType configAmmoType, int remainingAmmo )
	{
		if( (int)configAmmoType >= 0 && (int) configAmmoType < CurrentAmmunitionAvailable.Count ) 
		{
			CurrentAmmunitionAvailable[(int)configAmmoType] += remainingAmmo;
		}
	}
	public int TakeAmmo(AmmoType configAmmoType, int clipSize)
	{
		clipSize = Math.Max( clipSize, 0 );
		if ((int)configAmmoType >= 0 && (int)configAmmoType < CurrentAmmunitionAvailable.Count)
		{
			var current = CurrentAmmunitionAvailable[(int)configAmmoType];
			int ammoToReturn = Math.Min(current, clipSize);
			CurrentAmmunitionAvailable[(int)configAmmoType] -= ammoToReturn;

			return ammoToReturn;
		}
		return 0;
	}
}
