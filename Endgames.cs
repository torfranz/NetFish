using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Endgames
{
    public Dictionary<ulong, Endgame> endgames = new Dictionary<ulong, Endgame>();

    public Endgame probe(ulong key)
    {
        Endgame eg;
        if (endgames.TryGetValue(key, out eg))
        {
            return eg;
        }

        return null;
    }
    public Endgames()
    {
        endgames.Add(Endgame.key("KPK", Color.WHITE), new EndgameKPK(Color.WHITE));
        endgames.Add(Endgame.key("KNNK", Color.WHITE), new EndgameKNNK(Color.WHITE));
        endgames.Add(Endgame.key("KBNK", Color.WHITE), new EndgameKBNK(Color.WHITE));
        endgames.Add(Endgame.key("KRKP", Color.WHITE), new EndgameKRKP(Color.WHITE));
        endgames.Add(Endgame.key("KRKB", Color.WHITE), new EndgameKRKB(Color.WHITE));
        endgames.Add(Endgame.key("KRKN", Color.WHITE), new EndgameKRKN(Color.WHITE));
        endgames.Add(Endgame.key("KQKP", Color.WHITE), new EndgameKQKP(Color.WHITE));
        endgames.Add(Endgame.key("KQKR", Color.WHITE), new EndgameKQKR(Color.WHITE));
        endgames.Add(Endgame.key("KNPK", Color.WHITE), new EndgameKNPK(Color.WHITE));
        endgames.Add(Endgame.key("KNPKB", Color.WHITE), new EndgameKNPKB(Color.WHITE));
        endgames.Add(Endgame.key("KRPKR", Color.WHITE), new EndgameKRPKR(Color.WHITE));
        endgames.Add(Endgame.key("KRPKB", Color.WHITE), new EndgameKRPKB(Color.WHITE));
        endgames.Add(Endgame.key("KBPKB", Color.WHITE), new EndgameKBPKB(Color.WHITE));
        endgames.Add(Endgame.key("KBPKN", Color.WHITE), new EndgameKBPKN(Color.WHITE));
        endgames.Add(Endgame.key("KBPPKB", Color.WHITE), new EndgameKBPPKB(Color.WHITE));
        endgames.Add(Endgame.key("KRPPKRP", Color.WHITE), new EndgameKRPPKRP(Color.WHITE));

        endgames.Add(Endgame.key("KPK", Color.BLACK), new EndgameKPK(Color.BLACK));
        endgames.Add(Endgame.key("KNNK", Color.BLACK), new EndgameKNNK(Color.BLACK));
        endgames.Add(Endgame.key("KBNK", Color.BLACK), new EndgameKBNK(Color.BLACK));
        endgames.Add(Endgame.key("KRKP", Color.BLACK), new EndgameKRKP(Color.BLACK));
        endgames.Add(Endgame.key("KRKB", Color.BLACK), new EndgameKRKB(Color.BLACK));
        endgames.Add(Endgame.key("KRKN", Color.BLACK), new EndgameKRKN(Color.BLACK));
        endgames.Add(Endgame.key("KQKP", Color.BLACK), new EndgameKQKP(Color.BLACK));
        endgames.Add(Endgame.key("KQKR", Color.BLACK), new EndgameKQKR(Color.BLACK));
        endgames.Add(Endgame.key("KNPK", Color.BLACK), new EndgameKNPK(Color.BLACK));
        endgames.Add(Endgame.key("KNPKB", Color.BLACK), new EndgameKNPKB(Color.BLACK));
        endgames.Add(Endgame.key("KRPKR", Color.BLACK), new EndgameKRPKR(Color.BLACK));
        endgames.Add(Endgame.key("KRPKB", Color.BLACK), new EndgameKRPKB(Color.BLACK));
        endgames.Add(Endgame.key("KBPKB", Color.BLACK), new EndgameKBPKB(Color.BLACK));
        endgames.Add(Endgame.key("KBPKN", Color.BLACK), new EndgameKBPKN(Color.BLACK));
        endgames.Add(Endgame.key("KBPPKB", Color.BLACK), new EndgameKBPPKB(Color.BLACK));
        endgames.Add(Endgame.key("KRPPKRP", Color.BLACK), new EndgameKRPPKRP(Color.BLACK));
    }
}
