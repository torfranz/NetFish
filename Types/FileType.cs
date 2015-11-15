using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if PRIMITIVE
    using FileType = System.Int32;
#else

internal class FileType
{
    private int Value;

#region constructors

    internal FileType(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 7);
    }

#endregion

#region operators

    public static implicit operator int(FileType f)
    {
        return f.Value;
    }

    public static FileType operator ++(FileType f)
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

internal static class File
{

#if PRIMITIVE
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
    public static FileType Create(int value)
    {
        return value;
    }

#else
    internal static FileType FILE_A = new FileType(0);

    internal static FileType FILE_B = new FileType(1);

    internal static FileType FILE_C = new FileType(2);

    internal static FileType FILE_D = new FileType(3);

    internal static FileType FILE_E = new FileType(4);

    internal static FileType FILE_F = new FileType(5);

    internal static FileType FILE_G = new FileType(6);

    internal static FileType FILE_H = new FileType(7);

    public static FileType Create(int value)
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
    internal static FileType[] AllFiles = {FILE_A, FILE_B, FILE_C, FILE_D, FILE_E, FILE_F, FILE_G, FILE_H};

}