#if PRIMITIVE
using ColorType = System.Int32;
#endif

/// Material::Entry contains various information about a material configuration.
/// It contains a material imbalance evaluation, a function pointer to a special
/// endgame evaluation function (which in most cases is NULL, meaning that the
/// standard evaluation function will be used), and scale factors.
/// 
/// The scale factors are used to scale the evaluation score up or down. For
/// instance, in KRB vs KR endgames, the score is scaled down by a factor of 4,
/// which will result in scores of absolute value less than one pawn.
internal class MaterialEntry
{
    internal EndgameValue evaluationFunction;

    internal ushort[] factor = new ushort[Color.COLOR_NB];

    // side (e.g. KPKP, KBPsKs)
    internal Phase gamePhase;

    internal ulong key;

    internal EndgameScaleFactor[] scalingFunction = new EndgameScaleFactor[Color.COLOR_NB]; // Could be one for each

    internal short value;

    internal void reset()
    {
        evaluationFunction = null;
        factor = new ushort[Color.COLOR_NB];
        gamePhase = Phase.PHASE_ENDGAME;
        key = 0;
        scalingFunction = new EndgameScaleFactor[Color.COLOR_NB];
        value = 0;
    }

    internal Score imbalance()
    {
        return Score.make_score(value, value);
    }

    internal Phase game_phase()
    {
        return gamePhase;
    }

    internal bool specialized_eval_exists()
    {
        return evaluationFunction != null;
    }

    internal Value evaluate(Position pos)
    {
        return evaluationFunction.GetValue(pos);
    }

    // scale_factor takes a position and a color as input and returns a scale factor
    // for the given color. We have to provide the position in addition to the color
    // because the scale factor may also be a function which should be applied to
    // the position. For instance, in KBP vs K endgames, the scaling function looks
    // for rook pawns and wrong-colored bishops.
    internal ScaleFactor scale_factor(Position pos, ColorType c)
    {
        if (scalingFunction[c] == null)
        {
            return (ScaleFactor) (factor[c]);
        }
        return scalingFunction[c].GetScaleFactor(pos) == ScaleFactor.SCALE_FACTOR_NONE
            ? (ScaleFactor) (factor[c])
            : scalingFunction[c].GetScaleFactor(pos);
    }
}