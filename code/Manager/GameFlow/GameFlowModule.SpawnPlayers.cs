using System;
using System.Linq;
using MyGame;

namespace Sandbox.Manager.GameFlow;

public partial class GameFlowModule : AGameModule
{
	private void OnEnterSpawnPlayers()
	{
		if ( !Game.IsServer )
			return;

		// Get all of the spawnpoints
		var spawnpoints = Sandbox.Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoints = spawnpoints.OrderBy( x => Guid.NewGuid() ).ToList();
		int i = 0;
		foreach (IClient client in Game.Clients)
		{
			var pawn = new Pawn();
			client.Pawn = pawn;
			pawn.Respawn();
			pawn.DressFromClient( client );

			SpawnPoint randomSpawnPoint = i < randomSpawnPoints.Count ? randomSpawnPoints[i] : null;
			// if it exists, place the pawn there
			if ( randomSpawnPoint != null )
			{
				var tx = randomSpawnPoint.Transform;
				tx.Position += Vector3.Up * 50.0f; // raise it up
				pawn.Transform = tx;
				++i;
				i %= randomSpawnPoints.Count;
			}
		}
	}
}
