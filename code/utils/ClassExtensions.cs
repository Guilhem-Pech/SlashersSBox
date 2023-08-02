namespace Sandbox.Utils;

public static class BBoxExtensions 
{
	public static BBox Lerp(this BBox from, BBox to, float t, bool clamp = true)
	{
		Vector3 lerpMins = Vector3.Lerp(from.Mins, to.Mins, t, clamp);
		Vector3 lerpMaxs = Vector3.Lerp(from.Maxs, to.Maxs, t, clamp);

		return new BBox(lerpMins, lerpMaxs);
	}
	
}

public static class Vector3Extension
{
	public static float DistanceToLine( this Vector3 self, Vector3 start, Vector3 end, out Vector3 intersection )
	{
		var v = end - start;
		var w = self - start;

		var c1 = Vector3.Dot( w, v );
		if ( c1 <= 0 )
		{
			intersection = start;
			return self.Distance( start );
		}

		var c2 = Vector3.Dot( v, v );
		if ( c2 <= c1 )
		{
			intersection = end;
			return self.Distance( end );
		}

		var b = c1 / c2;
		var pb = start + b * v;

		intersection = pb;
		return self.Distance( pb );
	}
	
	public static float SqrDistanceToLine( this Vector3 self, Vector3 start, Vector3 end, out Vector3 intersection )
	{
		var v = end - start;
		var w = self - start;

		var c1 = Vector3.Dot( w, v );
		if ( c1 <= 0 )
		{
			intersection = start;
			return self.DistanceSquared( start );
		}

		var c2 = Vector3.Dot( v, v );
		if ( c2 <= c1 )
		{
			intersection = end;
			return self.DistanceSquared( end );
		}

		var b = c1 / c2;
		var pb = start + b * v;

		intersection = pb;
		return self.DistanceSquared( pb );
	}
	
}
