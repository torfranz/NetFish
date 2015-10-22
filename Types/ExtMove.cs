using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ExtMove
{
    public Move Move { get; }
    public Value Value { get; }

    public ExtMove(Move move, Value value)
    {
        this.Move = move;
        this.Value = value;
    }

    public static implicit operator Move(ExtMove move)
    {
        return move.Move;
    }

    public static bool operator <(ExtMove f, ExtMove s)
    {
        return f.Value < s.Value;
    }

    public static bool operator >(ExtMove f, ExtMove s)
    {
        return f.Value > s.Value;
    }

    public static bool operator ==(ExtMove f, ExtMove s)
    {
        return f.Move == s.Move;
    }

    public static bool operator !=(ExtMove f, ExtMove s)
    {
        return f.Move != s.Move;
    }

    public override string ToString()
    {
        return $"{Move},{Value}";
    }
};
