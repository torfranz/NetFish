using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class File
{
    internal const int FILE_A_C = 0;
    internal const int FILE_B_C = 1;
    internal const int FILE_C_C = 2;
    internal const int FILE_D_C = 3;
    internal const int FILE_E_C = 4;
    internal const int FILE_F_C = 5;
    internal const int FILE_G_C = 6;
    internal const int FILE_H_C = 7;
    internal const int FILE_NB_C = 8;

    internal static File FILE_A = new File(FILE_A_C);

    internal static File FILE_B = new File(FILE_B_C);

    internal static File FILE_C = new File(FILE_C_C);

    internal static File FILE_D = new File(FILE_D_C);

    internal static File FILE_E = new File(FILE_E_C);

    internal static File FILE_F = new File(FILE_F_C);

    internal static File FILE_G = new File(FILE_G_C);

    internal static File FILE_H = new File(FILE_H_C);

    internal static File[] AllFiles = {FILE_A, FILE_B, FILE_C, FILE_D, FILE_E, FILE_F, FILE_G, FILE_H};
    private int Value;

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File Create(int value)
    {
        switch (value)
        {
            case FILE_A_C: return FILE_A;
            case FILE_B_C: return FILE_B;
            case FILE_C_C: return FILE_C;
            case FILE_D_C: return FILE_D;
            case FILE_E_C: return FILE_E;
            case FILE_F_C: return FILE_F;
            case FILE_G_C: return FILE_G;
            case FILE_H_C: return FILE_H;
            default: throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private File(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= FILE_A_C && this.Value <= FILE_H_C);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static explicit operator int(File f)
    {
        return f.Value;
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