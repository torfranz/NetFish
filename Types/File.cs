using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if PRIMITIVE
    using FileT = System.Int32;
#else

internal class FileT
{
    private int Value;

    #region constructors

    internal FileT(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 7);
    }

    #endregion

    #region operators

    public static implicit operator int(FileT f)
    {
        return f.Value;
    }

    public static FileT operator ++(FileT f)
    {
        f.Value++;
        return f;
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }

    #endregion
}
#endif

internal static class File
{
#if PRIMITIVE
    internal const FileT FILE_A = 0;
    internal const FileT FILE_B = 1;
    internal const FileT FILE_C = 2;
    internal const FileT FILE_D = 3;
    internal const FileT FILE_E = 4;
    internal const FileT FILE_F = 5;
    internal const FileT FILE_G = 6;
    internal const FileT FILE_H = 7;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static FileT Create(int value)
    {
        return value;
    }

#else
    internal static FileT FILE_A = new FileT(0);

    internal static FileT FILE_B = new FileT(1);

    internal static FileT FILE_C = new FileT(2);

    internal static FileT FILE_D = new FileT(3);

    internal static FileT FILE_E = new FileT(4);

    internal static FileT FILE_F = new FileT(5);

    internal static FileT FILE_G = new FileT(6);

    internal static FileT FILE_H = new FileT(7);

    public static FileT Create(int value)
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

    internal static FileT[] AllFiles = { FILE_A, FILE_B, FILE_C, FILE_D, FILE_E, FILE_F, FILE_G, FILE_H };
}