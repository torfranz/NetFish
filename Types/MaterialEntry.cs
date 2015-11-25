﻿
#if PRIMITIVE
using ColorT = System.Int32;
using ValueT = System.Int32;
using ScoreT = System.Int32;
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
        this.evaluationFunction = null;
        this.factor = new ushort[Color.COLOR_NB];
        this.gamePhase = Phase.PHASE_ENDGAME;
        this.key = 0;
        this.scalingFunction = new EndgameScaleFactor[Color.COLOR_NB];
        this.value = 0;
    }

    internal ScoreT imbalance()
    {
        return Score.make_score(this.value, this.value);
    }

    internal Phase game_phase()
    {
        return this.gamePhase;
    }

    internal bool specialized_eval_exists()
    {
        return this.evaluationFunction != null;
    }

    internal ValueT evaluate(Position pos)
    {
        return this.evaluationFunction.GetValue(pos);
    }

    // scale_factor takes a position and a color as input and returns a scale factor
    // for the given color. We have to provide the position in addition to the color
    // because the scale factor may also be a function which should be applied to
    // the position. For instance, in KBP vs K endgames, the scaling function looks
    // for rook pawns and wrong-colored bishops.
    internal ScaleFactor scale_factor(Position pos, ColorT c)
    {
        if (this.scalingFunction[c] == null)
        {
            return (ScaleFactor)(this.factor[c]);
        }
        return this.scalingFunction[c].GetScaleFactor(pos) == ScaleFactor.SCALE_FACTOR_NONE
                   ? (ScaleFactor)(this.factor[c])
                   : this.scalingFunction[c].GetScaleFactor(pos);
    }
}