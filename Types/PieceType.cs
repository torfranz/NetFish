using System.Diagnostics;
using System.Runtime.CompilerServices;

internal struct PieceType
{
    internal const int BISHOP_C = 3;

    internal const int ROOK_C = 4;

    internal const int QUEEN_C = 5;

    internal static PieceType NO_PIECE_TYPE = new PieceType(0);

    internal static PieceType PAWN = new PieceType(1);

    internal static PieceType KNIGHT = new PieceType(2);

    internal static PieceType BISHOP = new PieceType(BISHOP_C);

    internal static PieceType ROOK = new PieceType(ROOK_C);

    internal static PieceType QUEEN = new PieceType(QUEEN_C);

    internal static PieceType KING = new PieceType(6);

    internal static PieceType ALL_PIECES = new PieceType(0);

    internal static PieceType PIECE_TYPE_NB = new PieceType(8);

    internal int Value
    {
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        get;
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private set;
    }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal PieceType(int value)
    {
        Value = value;
        Debug.Assert(Value >= 0 && Value <= 8);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceType operator +(PieceType v1, int v2)
    {
        return new PieceType(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceType operator +(int v1, PieceType v2)
    {
        return new PieceType(v1 + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceType operator -(PieceType v1, PieceType v2)
    {
        return new PieceType(v1.Value - v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(PieceType pt)
    {
        return pt.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(PieceType v1, PieceType v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(PieceType v1, PieceType v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceType operator ++(PieceType v1)
    {
        v1.Value += 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator bool(PieceType pt)
    {
        return pt.Value != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return Value.ToString();
    }

    /*
    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator -(PieceType v1)
    {
        return new PieceType(-v1.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator *(int v1, PieceType v2)
    {
        return new PieceType(v1 * v2.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator *(PieceType v1, int v2)
    {
        return new PieceType(v1.value * v2);
    }

    #endregion

    #region extended operators

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static int operator /(PieceType v1, PieceType v2)
    {
        return v1.value / v2.value;
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator /(PieceType v1, int v2)
    {
        return new PieceType(v1.value / v2);
    }
    */

    #endregion
}