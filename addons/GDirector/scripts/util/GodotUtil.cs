using System;
using Godot;

namespace Raele.GDirector;

public static class GodotUtil {
	public static Vector3 RotateToward(Vector3 from, Vector3 to, float maxRadians, Vector3? defaultAxis = null)
	{
		if (maxRadians < 0) {
			to *= -1;
			maxRadians *= -1;
		}
		float angle = from.AngleTo(to);
		if (angle < Mathf.Epsilon) {
			return to;
		} else if (Mathf.Pi - angle < Mathf.Epsilon) {
			return from.Rotated(defaultAxis ?? Vector3.Up, Math.Min(maxRadians, Mathf.Pi));
		}
		float weight = Mathf.Clamp(maxRadians / angle, 0, 1);
		return from.Slerp(to, weight);
	}

    public static bool CheckNormalsAreParallel(Vector3 a, Vector3 b, float epsilon = Mathf.Epsilon)
    {
		float dot = a.Dot(b);
		return dot <= -1 + epsilon || dot >= 1 - epsilon;
    }
}
