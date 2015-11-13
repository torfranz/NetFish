using System.Diagnostics;
using System.Runtime.CompilerServices;

internal struct File
{
    internal static File FILE_A = new File(0);

    internal static File FILE_B = new File(1);

    internal static File FILE_C = new File(2);

    internal static File FILE_D = new File(3);

    internal static File FILE_E = new File(4);

    internal static File FILE_F = new File(5);

    internal static File FILE_G = new File(6);

    internal static File FILE_H = new File(7);

    internal static File FILE_NB = new File(8);

    private int Value { get; set; }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal File(uint value)
        : this((int) value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal File(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= -8 && this.Value <= 8);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File operator +(File v1, File v2)
    {
        return new File(v1.Value + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File operator +(File v1, int v2)
    {
        return new File(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File operator +(int v1, File v2)
    {
        return new File(v1 + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File operator -(File v1, File v2)
    {
        return new File(v1.Value - v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File operator -(File v1, int v2)
    {
        return new File(v1.Value - v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(File f)
    {
        return f.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(File v1, File v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(File v1, File v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File operator ++(File v1)
    {
        v1.Value += 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion
   
}