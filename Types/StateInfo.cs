/// StateInfo struct stores information needed to restore a Position object to
/// its previous state when we retract a move. Whenever a move is made on the
/// board (by calling Position::do_move), a StateInfo object must be passed.
public class StateInfo
{
    public PieceType capturedType;

    public int castlingRights;

    public Bitboard checkersBB;

    public Square epSquare = Square.SQ_NONE;

    // Not copied when making a move
    public ulong key;

    public ulong materialKey;

    public Value[] nonPawnMaterial = new Value[Color.COLOR_NB];

    // Copied when making a move
    public ulong pawnKey;

    public int pliesFromNull;

    public StateInfo previous;

    public Score psq;

    public int rule50;

    public void copyFrom(StateInfo other)
    {
        pawnKey = other.pawnKey;
        materialKey = other.materialKey;
        nonPawnMaterial[0] = other.nonPawnMaterial[0];
        nonPawnMaterial[1] = other.nonPawnMaterial[1];
        castlingRights = other.castlingRights;
        rule50 = other.rule50;
        pliesFromNull = other.pliesFromNull;
        psq = other.psq;
        epSquare = other.epSquare;
        key = other.key;
        checkersBB = other.checkersBB;
        capturedType = other.capturedType;
    }
};