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
	[Net] private IList<Weapon> OwnedWeapons { get; set; } = new List<Weapon>();
	[Net, Change(nameof(OnActiveWeaponChanged))] public Weapon? ActiveWeapon { get; private set; }

	[ClientInput] private int InputWeaponSlot { get; set; } = 0;

	[Net, Predicted]
	private IList<int> CurrentAmmunition { get; set; } = Enumerable.Repeat(0, Enum.GetNames<AmmoType>().Length).ToList(); //Workaround the fact we can't replicate dictionary 

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
		Add( new Pistol() );
		Add( new Knife() );
	}
	
	public void Simulate( IClient client )
	{
		ActiveWeapon?.Simulate( client );
		
		if ( OwnedWeapons.Count > 0 )
		{
			if ( Input.Pressed( "SlotNext" ) )
			{
				InputWeaponSlot = (InputWeaponSlot + 1) % OwnedWeapons.Count;
			}
			else if ( Input.Pressed( "SlotPrev" ) )
			{
				InputWeaponSlot = (InputWeaponSlot + OwnedWeapons.Count - 1) % OwnedWeapons.Count;
			}

			for (var i = 0; i < OwnedWeapons.Count; ++i)
			{
				if(Input.Pressed( $"Slot{i + 1}"))
				{
					InputWeaponSlot = i;
				}
			}
		}
		
		if (Game.IsServer && InputWeaponSlot < OwnedWeapons.Count )
		{
			Weapon wantedWeapon = OwnedWeapons[InputWeaponSlot];
			if ( wantedWeapon != ActiveWeapon )
			{
				ChangeWeapon(wantedWeapon);
			}
		}
	}

	void ChangeWeapon( Weapon? newWeapon )
	{
		Game.AssertServer(  );
		ActiveWeapon?.ActiveEnd( Entity, ActiveWeapon.Owner != Entity );
		ActiveWeapon = newWeapon;
		newWeapon?.ActiveStart(Entity);
	}
	public void OnActiveWeaponChanged( Weapon? oldValue, Weapon? newValue )
	{
		oldValue?.ActiveEnd( Entity, oldValue.Owner != Entity );
		newValue?.ActiveStart(Entity);
	}
	public bool Add( Weapon weapon ) 
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

		if ( !weapon.IsValid() )
			return false;
		
		if ( weapon.Owner.IsValid() )
			return false;

		if ( !CanAdd( weapon ) )
			return false;
		
		if ( !weapon.CanCarry( Entity ) )
			return false;
		
		OwnedWeapons.Add( weapon );
		weapon.Parent = Entity;
		weapon.OnCarryStart( Entity );

		return true;
	}
	
	public bool IsCarryingType( Type t )
	{
		return OwnedWeapons.Any( x => x.GetType() == t );
	}
	
	public bool CanAdd( Weapon? entity )
	{
		return entity != null && entity.CanCarry( Entity );
	}
	
	public int AmmoCount( AmmoType configAmmoType )
	{
		if ( (int)configAmmoType >= 0 && (int) configAmmoType < CurrentAmmunition.Count )
		{
			return CurrentAmmunition[(int)configAmmoType];
		}
		return 0;
	}
	
	public void GiveAmmo( AmmoType configAmmoType, int remainingAmmo )
	{
		if( (int)configAmmoType >= 0 && (int) configAmmoType < CurrentAmmunition.Count ) 
		{
			CurrentAmmunition[(int)configAmmoType] += remainingAmmo;
		}
	}
	public int TakeAmmo(AmmoType configAmmoType, int clipSize)
	{
		clipSize = Math.Max( clipSize, 0 );
		if ((int)configAmmoType >= 0 && (int)configAmmoType < CurrentAmmunition.Count)
		{
			var current = CurrentAmmunition[(int)configAmmoType];
			int ammoToReturn = Math.Min(current, clipSize);
			CurrentAmmunition[(int)configAmmoType] -= ammoToReturn;

			return ammoToReturn;
		}
		return 0;
	}
}
