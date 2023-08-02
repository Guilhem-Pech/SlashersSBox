using System;

namespace Sandbox.pawn.PawnControllers.mechanics;

public partial class StaminaHandler : BaseNetworkable
{
	// Fields
	private TimeSince m_timeSinceStaminaGain = 0;
	private TimeSince m_timeSinceStaminaLoss = 0;

	public StaminaHandler( float maxStaminaValue, float waitingTimeBeforeRegain = 3f, float waitingTimeBeforeLose = 0f )
	{
		MaxStaminaValue = maxStaminaValue;
		WaitingTimeBeforeRegain = waitingTimeBeforeRegain;
		WaitingTimeBeforeLose = waitingTimeBeforeLose;
		Value = maxStaminaValue;
	}

	// Properties
	[Net, Predicted] public float Value { private set; get; } 
	public float MaxStaminaValue { set; get; }
	public float DesiredStaminaModifierRate { set; get; }
	public float WaitingTimeBeforeRegain { set; get; }
	public float WaitingTimeBeforeLose { set; get; } 

	// Methods
	public void Update( float _dt )
	{
		if ( DesiredStaminaModifierRate < 0 && m_timeSinceStaminaGain > WaitingTimeBeforeLose )
		{
			Value = Math.Max( 0, Value + DesiredStaminaModifierRate * _dt );
			m_timeSinceStaminaLoss = 0;
		}
		else if ( DesiredStaminaModifierRate > 0 && m_timeSinceStaminaLoss > WaitingTimeBeforeRegain )
		{
			Value = Math.Min( MaxStaminaValue, Value + DesiredStaminaModifierRate * _dt );
			m_timeSinceStaminaGain = 0;
		}
	}
	
	public void ForceCurrentStamina( float _value )
	{
		Value = Math.Clamp( _value, 0, MaxStaminaValue );
	}
}
