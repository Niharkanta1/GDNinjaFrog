using System;

/*
 * @author:		Nihar
 * @company:	DeadW0Lf Games
 */
public static class Utils
{
    public static float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    public static bool IsEqualApprox(float a, float b)
    {
        return IsEqualApprox(a, b, 0.00001);
    }

    private static bool IsEqualApprox(double left, double right, double delta)
    {
        return Math.Abs(left - right) < delta;
    }
}
