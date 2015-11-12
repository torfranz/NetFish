using System.Collections.Generic;

public class Endgames
{
    public Dictionary<ulong, EndgameScaleFactor> endgamesScaleFactor = new Dictionary<ulong, EndgameScaleFactor>();
    public Dictionary<ulong, EndgameValue> endgamesValue = new Dictionary<ulong, EndgameValue>();

    public Endgames()
    {
        endgamesValue.Add(Endgame.key("KPK", Color.WHITE), new EndgameKPK(Color.WHITE));
        endgamesValue.Add(Endgame.key("KNNK", Color.WHITE), new EndgameKNNK(Color.WHITE));
        endgamesValue.Add(Endgame.key("KBNK", Color.WHITE), new EndgameKBNK(Color.WHITE));
        endgamesValue.Add(Endgame.key("KRKP", Color.WHITE), new EndgameKRKP(Color.WHITE));
        endgamesValue.Add(Endgame.key("KRKB", Color.WHITE), new EndgameKRKB(Color.WHITE));
        endgamesValue.Add(Endgame.key("KRKN", Color.WHITE), new EndgameKRKN(Color.WHITE));
        endgamesValue.Add(Endgame.key("KQKP", Color.WHITE), new EndgameKQKP(Color.WHITE));
        endgamesValue.Add(Endgame.key("KQKR", Color.WHITE), new EndgameKQKR(Color.WHITE));
        endgamesValue.Add(Endgame.key("KPK", Color.BLACK), new EndgameKPK(Color.BLACK));
        endgamesValue.Add(Endgame.key("KNNK", Color.BLACK), new EndgameKNNK(Color.BLACK));
        endgamesValue.Add(Endgame.key("KBNK", Color.BLACK), new EndgameKBNK(Color.BLACK));
        endgamesValue.Add(Endgame.key("KRKP", Color.BLACK), new EndgameKRKP(Color.BLACK));
        endgamesValue.Add(Endgame.key("KRKB", Color.BLACK), new EndgameKRKB(Color.BLACK));
        endgamesValue.Add(Endgame.key("KRKN", Color.BLACK), new EndgameKRKN(Color.BLACK));
        endgamesValue.Add(Endgame.key("KQKP", Color.BLACK), new EndgameKQKP(Color.BLACK));
        endgamesValue.Add(Endgame.key("KQKR", Color.BLACK), new EndgameKQKR(Color.BLACK));

        endgamesScaleFactor.Add(Endgame.key("KNPK", Color.WHITE), new EndgameKNPK(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KNPKB", Color.WHITE), new EndgameKNPKB(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KRPKR", Color.WHITE), new EndgameKRPKR(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KRPKB", Color.WHITE), new EndgameKRPKB(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KBPKB", Color.WHITE), new EndgameKBPKB(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KBPKN", Color.WHITE), new EndgameKBPKN(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KBPPKB", Color.WHITE), new EndgameKBPPKB(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KRPPKRP", Color.WHITE), new EndgameKRPPKRP(Color.WHITE));
        endgamesScaleFactor.Add(Endgame.key("KNPK", Color.BLACK), new EndgameKNPK(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KNPKB", Color.BLACK), new EndgameKNPKB(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KRPKR", Color.BLACK), new EndgameKRPKR(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KRPKB", Color.BLACK), new EndgameKRPKB(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KBPKB", Color.BLACK), new EndgameKBPKB(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KBPKN", Color.BLACK), new EndgameKBPKN(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KBPPKB", Color.BLACK), new EndgameKBPPKB(Color.BLACK));
        endgamesScaleFactor.Add(Endgame.key("KRPPKRP", Color.BLACK), new EndgameKRPPKRP(Color.BLACK));
    }

    public EndgameValue probeEndgameValue(ulong key)
    {
        EndgameValue eg;
        if (endgamesValue.TryGetValue(key, out eg))
        {
            return eg;
        }

        return null;
    }

    public EndgameScaleFactor probeEndgameScaleFactor(ulong key)
    {
        EndgameScaleFactor eg;
        if (endgamesScaleFactor.TryGetValue(key, out eg))
        {
            return eg;
        }

        return null;
    }
}