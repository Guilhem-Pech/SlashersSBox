using System;

namespace Sandbox.pawn.PawnControllers.mechanics;

public partial class StaminaHandler : BaseNetworkable
{
	// Fields
	private TimeSince m_timeSinceStaminaGain = 0;
	private TimeSince m_timeSinceStaminaLoss = 0;

	public StaminaHandler( float maxStamina, float waitingTimeBeforeRegain = 3f, float waitingTimeBeforeLose = 0f )
	{
		MaxStamina = maxStamina;
		WaitingTimeBeforeRegain = waitingTimeBeforeRegain;
		WaitingTimeBeforeLose = waitingTimeBeforeLose;
		CurrentStamina = maxStamina;
	}

	// Properties
	[Net, Predicted] public float CurrentStamina { private set; get; } 
	public float MaxStamina { set; get; }
	public float DesiredStaminaModifierRate { set; get; }
	public float WaitingTimeBeforeRegain { set; get; }
	public float WaitingTimeBeforeLose { set; get; } 

	// Methods
	public void Update( float _dt )
	{
		if ( DesiredStaminaModifierRate < 0 && m_timeSinceStaminaGain > WaitingTimeBeforeLose )
		{
			CurrentStamina = Math.Max( 0, CurrentStamina + DesiredStaminaModifierRate * _dt );
			m_timeSinceStaminaLoss = 0;
		}
		else if ( DesiredStaminaModifierRate > 0 && m_timeSinceStaminaLoss > WaitingTimeBeforeRegain )
		{
			CurrentStamina = Math.Min( MaxStamina, CurrentStamina + DesiredStaminaModifierRate * _dt );
			m_timeSinceStaminaGain = 0;
		}
	}
	
	public void ForceCurrentStamina( float _value )
	{
		CurrentStamina = Math.Clamp( _value, 0, MaxStamina );
	}
}
