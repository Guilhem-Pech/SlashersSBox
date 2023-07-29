using System;
using System.Collections.Generic;
using System.Linq;
using MyGame;
using Pawn = Sandbox.pawn.Pawn;

namespace Sandbox.Manager.GameFlow
{
	public partial class GameFlowModule
	{
		private List<SpawnPoint> m_randomSpawnPoints = new();
		private int m_indexRandomSpawnpoint = 0;

		/// <summary>
		/// On entering the game, spawn all connected players at random points on the map.
		/// </summary>
		private void OnEnterSpawnPlayers()
		{
			if ( !Game.IsServer )
				return;

			m_onClientJoined += OnClientJoined;
			// Get all of the spawnpoints
			IEnumerable<SpawnPoint> spawnpoints = Sandbox.Entity.All.OfType<SpawnPoint>();

			// chose a random one
			m_randomSpawnPoints = spawnpoints.OrderBy( _ => Guid.NewGuid() ).ToList();
			foreach ( IClient client in Game.Clients )
			{
				var pawn = SpawnPawn( client );
				PlacePawnAtRandomPoint( pawn );
			}
		}

		/// <summary>
		/// When exiting the game, removes the actions associated with player joining.
		/// </summary>
		private void OnExitSpawnPlayers()
		{
			m_onClientJoined -= OnClientJoined;
		}

		/// <summary>
		/// When a new client joins, this function is invoked to spawn a new pawn for them and places it at a random point.
		/// </summary>
		private void OnClientJoined( object sender, ClientJoinedEvent e )
		{
			if ( !Game.IsServer )
				return;

			var pawn = SpawnPawn( e.Client );
			PlacePawnAtRandomPoint( pawn );
		}
		
		/// <summary>
		/// Spawns a new pawn for the given client and assigns it to them.
		/// </summary>
		private Pawn SpawnPawn( IClient client )
		{
			var pawn = new Pawn { };
			client.Pawn = pawn;
			pawn.Respawn();
			pawn.DressFromClient( client );
			return pawn;
		}

		/// <summary>
		/// Places the given pawn at a random spawn point on the map.
		/// </summary>
		private void PlacePawnAtRandomPoint( Pawn pawn )
		{
			SpawnPoint randomSpawnPoint = m_indexRandomSpawnpoint < m_randomSpawnPoints.Count
				? m_randomSpawnPoints[m_indexRandomSpawnpoint]
				: null;
			// if it exists, place the pawn there
			if ( randomSpawnPoint != null )
			{
				var tx = randomSpawnPoint.Transform;
				tx.Position += Vector3.Up * 50.0f; // raise it up
				pawn.Transform = tx;
				++m_indexRandomSpawnpoint;
				m_indexRandomSpawnpoint %= m_randomSpawnPoints.Count;
			}
		}
	}
}
