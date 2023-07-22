using System;
using System.Threading.Tasks;
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
		builder.AddState( States.InGame );
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
