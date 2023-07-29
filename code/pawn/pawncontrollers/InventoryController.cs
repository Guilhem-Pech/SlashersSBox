using MyGame;
using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

/// <summary>
/// The player's inventory holds a player's weapons, and holds the player's current weapon.
/// It also drives functionality such as weapon switching.
/// </summary>
public partial class InventoryController : EntityComponent<Pawn>, ISingletonComponent, IComponentSimulable, IEventListener
{
	public Weapon Pistol = new Pistol();
	
	[Net, Predicted]
	public Weapon? ActiveWeapon { get; set; } 
	
	protected override void OnActivate() 
	{
		// The pawn entity seems to not be fully spawned when OnActivate is called, so I need to do this trick :(
		Entity.EventDispatcher.RegisterEvent<EventOnRespawn>( this, OnRespawed );
	}

	private void OnRespawed( EventOnRespawn obj )
	{
		SetActiveWeapon( new Pistol() );
	}

	protected override void OnDeactivate()
	{
		SetActiveWeapon( null );
		Entity.EventDispatcher.UnregisterAllEvents( this );
		base.OnDeactivate();
	}
	
	public void Simulate( IClient client )
	{
		ActiveWeapon?.Simulate( client );
	}

	public void DropActiveWeapon()
	{
		ActiveWeapon = null;
	}
	public void SetActiveWeapon( Weapon? weapon )
	{
		ActiveWeapon?.OnHolster();
		ActiveWeapon = weapon;
		ActiveWeapon?.OnEquip( Entity );
	}
}
