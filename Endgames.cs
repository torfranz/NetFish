using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Endgames
{
    public Dictionary<string, Endgame>[] endgames = { new Dictionary<string, Endgame>() ,new Dictionary<string, Endgame>()};

    public Endgames()
    {
        endgames[Color.WHITE].Add("KPK", new EndgameKPK(Color.WHITE));
        endgames[Color.WHITE].Add("KNNK", new EndgameKNNK(Color.WHITE));
        endgames[Color.WHITE].Add("KBNK", new EndgameKBNK(Color.WHITE));
        endgames[Color.WHITE].Add("KRKP", new EndgameKRKP(Color.WHITE));
        endgames[Color.WHITE].Add("KRKB", new EndgameKRKB(Color.WHITE));
        endgames[Color.WHITE].Add("KRKN", new EndgameKRKN(Color.WHITE));
        endgames[Color.WHITE].Add("KQKP", new EndgameKQKP(Color.WHITE));
        endgames[Color.WHITE].Add("KQKR", new EndgameKQKR(Color.WHITE));
        endgames[Color.WHITE].Add("KNPK", new EndgameKNPK(Color.WHITE));
        endgames[Color.WHITE].Add("KNPKB", new EndgameKNPKB(Color.WHITE));
        endgames[Color.WHITE].Add("KRPKR", new EndgameKRPKR(Color.WHITE));
        endgames[Color.WHITE].Add("KRPKB", new EndgameKRPKB(Color.WHITE));
        endgames[Color.WHITE].Add("KBPKB", new EndgameKBPKB(Color.WHITE));
        endgames[Color.WHITE].Add("KBPKN", new EndgameKBPKN(Color.WHITE));
        endgames[Color.WHITE].Add("KBPPKB", new EndgameKBPPKB(Color.WHITE));
        endgames[Color.WHITE].Add("KRPPKRP", new EndgameKRPPKRP(Color.WHITE));

        endgames[Color.BLACK].Add("KPK", new EndgameKPK(Color.BLACK));
        endgames[Color.BLACK].Add("KNNK", new EndgameKNNK(Color.BLACK));
        endgames[Color.BLACK].Add("KBNK", new EndgameKBNK(Color.BLACK));
        endgames[Color.BLACK].Add("KRKP", new EndgameKRKP(Color.BLACK));
        endgames[Color.BLACK].Add("KRKB", new EndgameKRKB(Color.BLACK));
        endgames[Color.BLACK].Add("KRKN", new EndgameKRKN(Color.BLACK));
        endgames[Color.BLACK].Add("KQKP", new EndgameKQKP(Color.BLACK));
        endgames[Color.BLACK].Add("KQKR", new EndgameKQKR(Color.BLACK));
        endgames[Color.BLACK].Add("KNPK", new EndgameKNPK(Color.BLACK));
        endgames[Color.BLACK].Add("KNPKB", new EndgameKNPKB(Color.BLACK));
        endgames[Color.BLACK].Add("KRPKR", new EndgameKRPKR(Color.BLACK));
        endgames[Color.BLACK].Add("KRPKB", new EndgameKRPKB(Color.BLACK));
        endgames[Color.BLACK].Add("KBPKB", new EndgameKBPKB(Color.BLACK));
        endgames[Color.BLACK].Add("KBPKN", new EndgameKBPKN(Color.BLACK));
        endgames[Color.BLACK].Add("KBPPKB", new EndgameKBPPKB(Color.BLACK));
        endgames[Color.BLACK].Add("KRPPKRP", new EndgameKRPPKRP(Color.BLACK));

    }
}
