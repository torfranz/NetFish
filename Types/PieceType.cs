using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct PieceType
{
    public const int BISHOP_C = 3;

    public const int ROOK_C = 4;

    public const int QUEEN_C = 5;

    public static PieceType NO_PIECE_TYPE = new PieceType(0);

    public static PieceType PAWN = new PieceType(1);

    public static PieceType KNIGHT = new PieceType(2);

    public static PieceType BISHOP = new PieceType(BISHOP_C);

    public static PieceType ROOK = new PieceType(ROOK_C);

    public static PieceType QUEEN = new PieceType(QUEEN_C);

    public static PieceType KING = new PieceType(6);

    public static PieceType ALL_PIECES = new PieceType(0);

    public static PieceType PIECE_TYPE_NB = new PieceType(8);

    private int Value { get; set; }

    #region constructors

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public PieceType(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 8);
    }

    #endregion

    #region base operators

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static PieceType operator +(PieceType v1, PieceType v2)
    {
        return new PieceType(v1.Value + v2.Value);
    }

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

    public static PieceType operator -(PieceType v1, int v2)
    {
        return new PieceType(v1.Value - v2);
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

    public override string ToString()
    {
        return this.Value.ToString();
    }

    /*
    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static PieceType operator -(PieceType v1)
    {
        return new PieceType(-v1.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static PieceType operator *(int v1, PieceType v2)
    {
        return new PieceType(v1 * v2.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static PieceType operator *(PieceType v1, int v2)
    {
        return new PieceType(v1.value * v2);
    }

    #endregion

    #region extended operators

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static int operator /(PieceType v1, PieceType v2)
    {
        return v1.value / v2.value;
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static PieceType operator /(PieceType v1, int v2)
    {
        return new PieceType(v1.value / v2);
    }
    */

    #endregion
}