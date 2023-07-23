using MyGame;
using Sandbox.Manager.GameFlow;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Sandbox;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
[Prefab]
public partial class Slashers : Sandbox.GameManager
{
	
	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public Slashers() 
	{
		if ( Game.IsServer )
		{
			Components.Create<GameFlowModule>();
		}
		
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}
	}

	/// <summary>
	/// A client has joined the server.
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		
	}
}

