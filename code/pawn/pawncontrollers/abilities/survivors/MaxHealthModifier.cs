namespace Sandbox.pawn.pawncontrollers.abilities.survivors;

public class MaxHealthModifier : IAbility
{
	/// <summary>
	/// Max Health modifier ability
	/// </summary>
	/// <param name="maxHealth">The new max health (by default is 100 pv)</param>
	public MaxHealthModifier( float maxHealth )
	{
		MaxHealth = maxHealth;
	}
	private Pawn? Pawn { get; set; }
	public float MaxHealth { get; private set; }

	private float m_oldValue = 0f;
	
	public void OnActivate( Pawn pawn )
	{
		Pawn = pawn;
		m_oldValue = Pawn.MaxHealth;
		Pawn.MaxHealth = MaxHealth;
		Pawn.Health = MathX.Lerp( 0, MaxHealth, m_oldValue / Pawn.Health ); // Will probably only ever be MaxHealth but who knows
	}

	public void OnDeactivate()
	{
		if ( Pawn != null )
		{
			Pawn.MaxHealth = m_oldValue;
		}
	}
}
