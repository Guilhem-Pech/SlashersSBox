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
