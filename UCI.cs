using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UCI
{
    /// UCI::square() converts a Square to a string in algebraic notation (g1, a7, etc.)
    public static string square(Square s)
    {
        return $"{(char)('a' + Square.file_of(s))}{(char)('1' + Square.rank_of(s))}";
    }
}
