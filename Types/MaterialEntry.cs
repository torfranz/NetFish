﻿/// Material::Entry contains various information about a material configuration.
/// It contains a material imbalance evaluation, a function pointer to a special
/// endgame evaluation function (which in most cases is NULL, meaning that the
/// standard evaluation function will be used), and scale factors.
/// 
/// The scale factors are used to scale the evaluation score up or down. For
/// instance, in KRB vs KR endgames, the score is scaled down by a factor of 4,
/// which will result in scores of absolute value less than one pawn.
public class MaterialEntry
{
    private Endgame evaluationFunction;
    private readonly ushort[] factor = new ushort[Color.COLOR_NB];
    // side (e.g. KPKP, KBPsKs)
    private Phase gamePhase;
    private ulong key;
    private readonly Endgame[] scalingFunction = new Endgame[Color.COLOR_NB]; // Could be one for each
    private short value;

    public Score imbalance()
    {
        return Score.make_score(value, value);
    }

    public Phase game_phase()
    {
        return gamePhase;
    }

    public bool specialized_eval_exists()
    {
        return evaluationFunction != null;
    }

    public Value evaluate(Position pos)
    {
        return evaluationFunction.GetValue(pos);
    }

    // scale_factor takes a position and a color as input and returns a scale factor
    // for the given color. We have to provide the position in addition to the color
    // because the scale factor may also be a function which should be applied to
    // the position. For instance, in KBP vs K endgames, the scaling function looks
    // for rook pawns and wrong-colored bishops.
    public ScaleFactor scale_factor(Position pos, Color c)
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