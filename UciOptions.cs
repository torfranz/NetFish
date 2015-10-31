using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

internal delegate void OnChangeUCIOption(UCIOption opt);

internal sealed class UCIOption
{
    internal string defaultValue, currentValue, type;

    internal int idx;

    internal int min, max;

    internal OnChangeUCIOption on_change;

    internal UCIOption(int index, OnChangeUCIOption fn)
    {
        type = "button";
        idx = index;
        on_change = fn;
    }

    internal UCIOption(int index, string v, OnChangeUCIOption fn)
        : this(index, fn)
    {
        type = "string";
        defaultValue = currentValue = v;
    }

    internal UCIOption(int index, bool v, OnChangeUCIOption fn)
        : this(index, fn)
    {
        type = "check";
        defaultValue = currentValue = (v ? "true" : "false");
    }

    internal UCIOption(int index, int v, int minv, int maxv, OnChangeUCIOption fn)
        : this(index, fn)
    {
        type = "spin";
        min = minv;
        max = maxv;
        defaultValue = currentValue = v.ToString();
    }

    internal string name { get; set; }

    internal string v
    {
        get { return currentValue; }
        set
        {
            Debug.Assert(type.Length > 0);

            if (((type == "button") || (value != null))
                && ((type != "check") || (value == "true" || value == "false"))
                && ((type != "spin") || ((int.Parse(value) >= min && int.Parse(value) <= max))))
            {
                if (type != "button")
                {
                    currentValue = value;
                }

                if (on_change != null)
                {
                    on_change(this);
                }
            }
        }
    }

    internal sealed class UCIOptionIndexComparer : IComparer<UCIOption>
    {
        int IComparer<UCIOption>.Compare(UCIOption x, UCIOption y)
        {
            return x.idx.CompareTo(y.idx);
        }
    }
}

/// 'On change' actions, triggered by an option's value change
internal static class UCIOptionChanges
{
    internal static void on_tb_path(UCIOption opt)
    {
        //TODO: enable call, Tablebases::init(o);
        //Tablebases::init(o);
    }

    internal static void on_threads(UCIOption opt)
    {
        ThreadPool.read_uci_options(null);
    }

    internal static void on_hash_size(UCIOption opt)
    {
        TranspositionTable.resize(uint.Parse(opt.v));
    }

    internal static void on_clear_hash(UCIOption opt)
    {
        //TODO: enable call , Search::reset();
        //Search::reset();
    }
}

public class OptionMap
{
    private readonly Dictionary<string, UCIOption> o = new Dictionary<string, UCIOption>();

    private OptionMap()
    {
#if X64
            int MaxHashMB = 1024 * 1024;
#else
        var MaxHashMB = 2048;
#endif

        var idx = 0;
        Add("Contempt", new UCIOption(idx++, 0, -100, 100, null));
        Add("Min Split Depth", new UCIOption(idx++, 5, 0, 12, UCIOptionChanges.on_threads));
        Add("Threads", new UCIOption(idx++, 1, 1, _.MAX_THREADS, UCIOptionChanges.on_threads));
        Add("Hash", new UCIOption(idx++, 16, 1, MaxHashMB, UCIOptionChanges.on_hash_size));
        Add("Clear Hash", new UCIOption(idx++, UCIOptionChanges.on_clear_hash));
        Add("Ponder", new UCIOption(idx++, true, null));
        Add("MultiPV", new UCIOption(idx++, 1, 1, 500, null));
        Add("Skill Level", new UCIOption(idx++, 20, 0, 20, null));
        Add("Move Overhead", new UCIOption(idx++, 30, 0, 5000, null));
        Add("Minimum Thinking Time", new UCIOption(idx++, 32, 0, 5000, null));
        Add("Slow Mover", new UCIOption(idx++, 80, 10, 1000, null));
        Add("nodestime", new UCIOption(idx++, 0, 0, 10000, null));
        Add("UCI_Chess960", new UCIOption(idx++, false, null));
        Add("SyzygyPath", new UCIOption(idx++, "<empty>", UCIOptionChanges.on_tb_path));
        Add("SyzygyProbeDepth", new UCIOption(idx++, 1, 1, 100, null));
        Add("Syzygy50MoveRule", new UCIOption(idx++, true, null));
        Add("SyzygyProbeLimit", new UCIOption(idx++, 6, 0, 6, null));
    }

    internal UCIOption this[string name]
    {
        get { return o[name.ToLowerInvariant()]; }
    }


    internal bool Contains(string name)
    {
        return o.ContainsKey(name.ToLowerInvariant());
    }

    /// operator
    /// <
    /// <() is used to print all the options default values in chronological
    ///     insertion order ( the idx field) and in the format defined by the UCI protocol.
    public override string ToString()
    {
        var list = new List<UCIOption>();
        list.AddRange(o.Values);
        list.Sort(new UCIOption.UCIOptionIndexComparer());
        var sb = new StringBuilder();
        foreach (var opt in list)
        {
            sb.Append("\noption name ").Append(opt.name).Append(" type ").Append(opt.type);
            if (opt.type != "button")
            {
                sb.Append(" default ").Append(opt.defaultValue);
            }
            if (opt.type == "spin")
            {
                sb.Append(" min ").Append(opt.min).Append(" max ").Append(opt.max);
            }
        }
        return sb.ToString();
    }

    private void Add(string optionName, UCIOption option)
    {
        var lname = optionName.ToLowerInvariant();
        option.name = optionName;
        o.Add(lname, option);
    }

    #region Singleton

    private static readonly object _instanceLock = new object();

    private static OptionMap _instance;

    internal static OptionMap Instance
    {
        get
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new OptionMap();
                }
            }
            return _instance;
        }
    }

    #endregion
}