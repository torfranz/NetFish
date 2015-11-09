using System.Collections.Generic;

public class Endgames
{
    public Dictionary<ulong, EndgameValue> endgamesValue = new Dictionary<ulong, EndgameValue>();
    public Dictionary<ulong, EndgameScaleFactor> endgamesScaleFactor = new Dictionary<ulong, EndgameScaleFactor>();

    public Endgames()
    {
        this.endgamesValue.Add(Endgame.key("KPK", Color.WHITE), new EndgameKPK(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KNNK", Color.WHITE), new EndgameKNNK(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KBNK", Color.WHITE), new EndgameKBNK(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KRKP", Color.WHITE), new EndgameKRKP(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KRKB", Color.WHITE), new EndgameKRKB(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KRKN", Color.WHITE), new EndgameKRKN(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KQKP", Color.WHITE), new EndgameKQKP(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KQKR", Color.WHITE), new EndgameKQKR(Color.WHITE));
        this.endgamesValue.Add(Endgame.key("KPK", Color.BLACK), new EndgameKPK(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KNNK", Color.BLACK), new EndgameKNNK(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KBNK", Color.BLACK), new EndgameKBNK(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KRKP", Color.BLACK), new EndgameKRKP(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KRKB", Color.BLACK), new EndgameKRKB(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KRKN", Color.BLACK), new EndgameKRKN(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KQKP", Color.BLACK), new EndgameKQKP(Color.BLACK));
        this.endgamesValue.Add(Endgame.key("KQKR", Color.BLACK), new EndgameKQKR(Color.BLACK));
        
        this.endgamesScaleFactor.Add(Endgame.key("KNPK", Color.WHITE), new EndgameKNPK(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KNPKB", Color.WHITE), new EndgameKNPKB(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KRPKR", Color.WHITE), new EndgameKRPKR(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KRPKB", Color.WHITE), new EndgameKRPKB(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KBPKB", Color.WHITE), new EndgameKBPKB(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KBPKN", Color.WHITE), new EndgameKBPKN(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KBPPKB", Color.WHITE), new EndgameKBPPKB(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KRPPKRP", Color.WHITE), new EndgameKRPPKRP(Color.WHITE));
        this.endgamesScaleFactor.Add(Endgame.key("KNPK", Color.BLACK), new EndgameKNPK(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KNPKB", Color.BLACK), new EndgameKNPKB(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KRPKR", Color.BLACK), new EndgameKRPKR(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KRPKB", Color.BLACK), new EndgameKRPKB(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KBPKB", Color.BLACK), new EndgameKBPKB(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KBPKN", Color.BLACK), new EndgameKBPKN(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KBPPKB", Color.BLACK), new EndgameKBPPKB(Color.BLACK));
        this.endgamesScaleFactor.Add(Endgame.key("KRPPKRP", Color.BLACK), new EndgameKRPPKRP(Color.BLACK));
    }

    public EndgameValue probeEndgameValue(ulong key)
    {
        EndgameValue eg;
        if (this.endgamesValue.TryGetValue(key, out eg))
        {
            return eg;
        }

        return null;
    }

    public EndgameScaleFactor probeEndgameScaleFactor(ulong key)
    {
        EndgameScaleFactor eg;
        if (this.endgamesScaleFactor.TryGetValue(key, out eg))
        {
            return eg;
        }

        return null;
    }
}