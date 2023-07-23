using System;
using System.Linq;
using MyGame;
using Sandbox.Utils;

namespace Sandbox.Manager;

[Prefab]
public class GameFlowModule : AGameModule
{
	private HFSM<States, TransitionEvents> m_hfsm;

	public string DebugCurrentState => m_hfsm != null ? m_hfsm.GetDebugCurrentStateName() : string.Empty;

	protected override void OnActivate()
	{
		BuildHFSM();
		base.OnActivate();
	}
	
	[GameEvent.Tick]
	private void OnUpdate()
	{
		m_hfsm.Update();
	}

	protected override void OnDeactivate()
	{
		m_hfsm.Stop();
	}

	private void BuildHFSM()
	{
		HfsmBuilder<States, TransitionEvents> builder = new HfsmBuilder<States, TransitionEvents>();
		
		builder.AddState( States.WaitForPlayers );
		{
			builder.AddTransition( States.WaitForPlayers, States.InGame, TransitionEvents.Trigger );
		}
		builder.AddState( States.InGame , OnEnterGame);
		{
			builder.AddState( States.PrepareObjectives, States.InGame );
			{
				builder.AddTransition( States.PrepareObjectives, States.SpawnPlayers, TransitionEvents.Trigger );
			}
			builder.AddState( States.SpawnPlayers, States.InGame );
			{
				builder.AddTransition( States.SpawnPlayers, States.Gameplay, TransitionEvents.Trigger );
			}
			builder.AddState( States.Gameplay, States.InGame );
			{
				builder.AddState( States.Jerrycans, States.Gameplay );
				{
					builder.AddTransition( States.Jerrycans, States.Generator, TransitionEvents.Trigger );
				}
				builder.AddState( States.Generator, States.Gameplay );
				{
					builder.AddTransition( States.Generator, States.Radio, TransitionEvents.Trigger );
				}
				builder.AddState( States.Radio, States.Gameplay );
				{
					builder.AddTransition( States.Radio, States.Police, TransitionEvents.Trigger );
				}
				builder.AddState( States.Police, States.Gameplay );
				
				builder.AddTransition( States.Gameplay, States.Results, TransitionEvents.Trigger );
			}
			builder.AddState( States.Results );
			{
				builder.AddTransition( States.Results, States.VoteMap, TransitionEvents.Trigger );
			}
			builder.AddState( States.VoteMap );
			{
				builder.AddTransition( States.VoteMap, States.WaitForPlayers, TransitionEvents.Trigger );
			}
		}
		m_hfsm = builder.Build();
		m_hfsm.EnableDebugLog = Enabled;
	}

	private void OnEnterGame()
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

enum States
{
	WaitForPlayers,
	InGame,
	PrepareObjectives,
	SpawnPlayers,
	Gameplay,
	Jerrycans,
	Generator,
	Radio,
	Police,
	Results,
	VoteMap
};

enum TransitionEvents
{
	Invalid,
	Trigger
}
