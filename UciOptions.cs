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
        this.type = "button";
        this.idx = index;
        this.on_change = fn;
    }

    internal UCIOption(int index, string v, OnChangeUCIOption fn)
        : this(index, fn)
    {
        this.type = "string";
        this.defaultValue = this.currentValue = v;
    }

    internal UCIOption(int index, bool v, OnChangeUCIOption fn)
        : this(index, fn)
    {
        this.type = "check";
        this.defaultValue = this.currentValue = (v ? "true" : "false");
    }

    internal UCIOption(int index, int v, int minv, int maxv, OnChangeUCIOption fn)
        : this(index, fn)
    {
        this.type = "spin";
        this.min = minv;
        this.max = maxv;
        this.defaultValue = this.currentValue = v.ToString();
    }

    internal string name { get; set; }

    internal string v
    {
        get
        {
            return this.currentValue;
        }
        set
        {
            Debug.Assert(this.type.Length > 0);

            if (((this.type == "button") || (value != null))
                && ((this.type != "check") || (value == "true" || value == "false"))
                && ((this.type != "spin") || ((int.Parse(value) >= this.min && int.Parse(value) <= this.max))))
            {
                if (this.type != "button")
                {
                    this.currentValue = value;
                }

                if (this.on_change != null)
                {
                    this.on_change(this);
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
        Search.reset();
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
        this.Add("Contempt", new UCIOption(idx++, 0, -100, 100, null));
        this.Add("Min Split Depth", new UCIOption(idx++, 5, 0, 12, UCIOptionChanges.on_threads));
        this.Add("Threads", new UCIOption(idx++, 1, 1, _.MAX_THREADS, UCIOptionChanges.on_threads));
        this.Add("Hash", new UCIOption(idx++, 16, 1, MaxHashMB, UCIOptionChanges.on_hash_size));
        this.Add("Clear Hash", new UCIOption(idx++, UCIOptionChanges.on_clear_hash));
        this.Add("Ponder", new UCIOption(idx++, true, null));
        this.Add("MultiPV", new UCIOption(idx++, 1, 1, 500, null));
        this.Add("Skill Level", new UCIOption(idx++, 20, 0, 20, null));
        this.Add("Move Overhead", new UCIOption(idx++, 30, 0, 5000, null));
        this.Add("Minimum Thinking Time", new UCIOption(idx++, 32, 0, 5000, null));
        this.Add("Slow Mover", new UCIOption(idx++, 80, 10, 1000, null));
        this.Add("nodestime", new UCIOption(idx++, 0, 0, 10000, null));
        this.Add("UCI_Chess960", new UCIOption(idx++, false, null));
        this.Add("SyzygyPath", new UCIOption(idx++, "<empty>", UCIOptionChanges.on_tb_path));
        this.Add("SyzygyProbeDepth", new UCIOption(idx++, 1, 1, 100, null));
        this.Add("Syzygy50MoveRule", new UCIOption(idx++, true, null));
        this.Add("SyzygyProbeLimit", new UCIOption(idx++, 6, 0, 6, null));
    }

    internal UCIOption this[string name]
    {
        get
        {
            return this.o[name.ToLowerInvariant()];
        }
    }

    internal bool Contains(string name)
    {
        return this.o.ContainsKey(name.ToLowerInvariant());
    }

    /// operator
    /// <
    /// <() is used to print all the options default values in chronological
    ///     insertion order ( the idx field) and in the format defined by the UCI protocol.
    public override string ToString()
    {
        var list = new List<UCIOption>();
        list.AddRange(this.o.Values);
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
        this.o.Add(lname, option);
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