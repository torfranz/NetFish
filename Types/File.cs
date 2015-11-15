using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if IMPLICIT
    using File = System.Int32;
#else

internal class File
{
    private int Value;

#region constructors

    internal File(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 7);
    }

#endregion

#region operators

    public static implicit operator int(File f)
    {
        return f.Value;
    }

    public static File operator ++(File f)
    {
        f.Value++;
        return f;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

#endregion
   
}
#endif

internal static class FileConstants
{

#if IMPLICIT
    internal const int FILE_A = 0;
    internal const int FILE_B = 1;
    internal const int FILE_C = 2;
    internal const int FILE_D = 3;
    internal const int FILE_E = 4;
    internal const int FILE_F = 5;
    internal const int FILE_G = 6;
    internal const int FILE_H = 7;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static File Create(int value)
    {
        return value;
    }

#else
    internal static File FILE_A = new File(0);

    internal static File FILE_B = new File(1);

    internal static File FILE_C = new File(2);

    internal static File FILE_D = new File(3);

    internal static File FILE_E = new File(4);

    internal static File FILE_F = new File(5);

    internal static File FILE_G = new File(6);

    internal static File FILE_H = new File(7);

    public static File Create(int value)
    {
        switch (value)
        {
            case 0:
                return FILE_A;
            case 1:
                return FILE_B;
            case 2:
                return FILE_C;
            case 3:
                return FILE_D;
            case 4:
                return FILE_E;
            case 5:
                return FILE_F;
            case 6:
                return FILE_G;
            case 7:
                return FILE_H;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
#endif

    internal const int FILE_NB = 8;
    internal static File[] AllFiles = {FILE_A, FILE_B, FILE_C, FILE_D, FILE_E, FILE_F, FILE_G, FILE_H};

}