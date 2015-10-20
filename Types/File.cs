using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct File
{
    public static File FILE_A = new File(0);

    public static File FILE_B = new File(1);

    public static File FILE_C = new File(2);

    public static File FILE_D = new File(3);

    public static File FILE_E = new File(4);

    public static File FILE_F = new File(5);

    public static File FILE_G = new File(6);

    public static File FILE_H = new File(7);

    public static File FILE_NB = new File(8);

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File(uint value)
        : this((int)value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= -8 && this.Value <= 8);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(File f)
    {
        return f.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(File v1, File v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(File v1, File v2)
    {
        return v1.Value != v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator ++(File v1)
    {
        return new File(v1.Value + 1);
    }

    public override string ToString()
    {
        return this.Value.ToString();
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