using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ExtMove
{
    public Move move;
    public Value value;

    public static implicit operator Move(ExtMove move)
    {
        return move.move;
    }

    public static bool operator <(ExtMove f, ExtMove s)
    {
        return f.value < s.value;
    }

    public static bool operator >(ExtMove f, ExtMove s)
    {
        return f.value > s.value;
    }

    public static bool operator ==(ExtMove f, ExtMove s)
    {
        return f.move == s.move;
    }

    public static bool operator !=(ExtMove f, ExtMove s)
    {
        return f.move != s.move;
    }
};
