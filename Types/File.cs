using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct File
{
    public const int FILE_A = 0;

    public const int FILE_B = 1;

    public const int FILE_C = 2;

    public const int FILE_D = 3;

    public const int FILE_E = 4;

    public const int FILE_F = 5;

    public const int FILE_G = 6;

    public const int FILE_H = 7;

    public const int FILE_NB = 8;

    public int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File(File file)
        : this(file.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 8);
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator +(File v1, File v2)
    {
        return new File(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator +(File v1, int v2)
    {
        return new File(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator +(int v1, File v2)
    {
        return new File(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator -(File v1, File v2)
    {
        return new File(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator -(File v1, int v2)
    {
        return new File(v1.Value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator -(File v1)
    {
        return new File(-v1.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator *(int v1, File v2)
    {
        return new File(v1 * v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator *(File v1, int v2)
    {
        return new File(v1.Value * v2);
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(File v1, File v2)
    {
        return v1.Value / v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator /(File v1, int v2)
    {
        return new File(v1.Value / v2);
    }

    #endregion
}