using VisualFA;

internal partial class Program
{
    [CmdArg(Ordinal = 0)]
    static TextReader Input { get; set; } = Console.In;
    [CmdArg]
    static TextWriter Output { get; set; } = Console.Out;
    struct _Field
    {
        public string RawType;
        public string Name;
        public int RawLength;
    }
    static void _Expecting(FAMatch match, params int[] ids)
    {
        int i;
        for(i  = 0; i < ids.Length;++i)
        {
            if (match.SymbolId == ids[i])
                break;
        }
        if(i==ids.Length)
        {
            throw new Exception(string.Format("Unexpected symbol in input. at line {0}, column {1}",match.Line, match.Column));
        }
    }
    static bool _SkipToId(IEnumerator<FAMatch> e, string id)
    {
        while(_SkipTo(e,identifier) && e.Current.Value!=id)
        {
            if(!e.MoveNext())
            {
                return false;
            }
        }
        return e.Current.SymbolId==identifier && e.Current.Value==id ;
    }
    static bool _SkipTo(IEnumerator<FAMatch> e, int id)
    {
        var result = true;
        while (e.Current.SymbolId != id && (result = e.MoveNext())) ;
        return result;
    }
    static void Run()
    {
        
        var structs = new Dictionary<string,List<_Field>>();
        using (var e = Lex(Input).GetEnumerator())
        {
            if(!e.MoveNext())
            {
                Console.Error.WriteLine("Header was empty of significant content.");
                return;
            }
            while(_SkipToId(e,"struct"))
            {
                if(!e.MoveNext())
                {
                    Console.Error.WriteLine("Unterminated struct");
                    return;
                }
                _Expecting(e.Current, identifier);
                string name = e.Current.Value;
                _Field f = default;
                if (!_SkipTo(e,lbrace))
                {
                    Console.Error.WriteLine("Expected {");
                    return;
                }
                if(!_SkipTo(e,identifier))
                {
                    Console.Error.WriteLine("Unterminated or empty struct");
                    return;
                }
                var fields = new List<_Field>();
                while (_SkipTo(e,identifier))
                {
                    if (e.Current.Value == "constexpr")
                    {
                        if (!_SkipTo(e, semi))
                        {
                            Console.Error.WriteLine("Unterminated field");
                            return;
                            
                        }
                        continue;
                    }
                    f.RawType = e.Current.Value;
                    if(!e.MoveNext())
                    {
                        Console.Error.WriteLine("Unterminated field");
                        return;
                    }
                    if(!_SkipTo(e,identifier))
                    {
                        _Expecting(e.Current, identifier);
                    }
                    f.Name = e.Current.Value;
                    if(!e.MoveNext())
                    {
                        Console.Error.WriteLine("Unterminated field");
                        return;
                    }
                    if(e.Current.SymbolId==lbracket)
                    {
                        if(!e.MoveNext())
                        {
                            Console.Error.WriteLine("Unterminated field");
                            return;
                        }
                        _Expecting(e.Current, integer);
                        f.RawLength = int.Parse(e.Current.Value);
                        if (!e.MoveNext())
                        {
                            Console.Error.WriteLine("Unterminated field");
                            return;
                        }
                        _Expecting(e.Current, rbracket);
                        if (!e.MoveNext())
                        {
                            Console.Error.WriteLine("Unterminated field");
                            return;
                        }
                        
                    } else if(e.Current.SymbolId==equals)
                    {
                        if(!_SkipTo(e,rbracket))
                        {
                            Console.Error.WriteLine("Unterminated struct");
                            return;
                        }
                        name = null;
                        break;
                    }
                    _Expecting(e.Current, semi);
                    if(!e.MoveNext())
                    {
                        Console.Error.WriteLine("Unterminated field");
                        return;

                    }
                    fields.Add(f);
                    f = default;
                    if(e.Current.SymbolId==rbrace)
                    {
                        break;
                    }
                }
                if (name != null)
                {
                    structs.Add(name, fields);
                }
            }
        }
        foreach(var s in structs)
        {
            if (s.Key == "data_packet" || s.Key == "command_packet" || s.Key == "config_packet")
            {
                foreach (var f in s.Value)
                {
                    Output.Write("\""+f.Name+"\", ");
                    Output.Write("\"" + f.RawType + "\", ");
                    Output.WriteLine(f.RawLength==0?4:f.RawLength);
                }
                Output.WriteLine();
            }
        }
    }
    
    [FARule(@"\/\/[^\n]*")]  // 0
    [FARule(@"\/\*",BlockEnd = @"\*\/")] // 1
    [FARule(@"#[^\n]*")] // 2
    [FARule(@"[ \r\n\t]+)")] // 3
    [FARule(@"[A-Z_a-z][0-9A-Z_a-z]*")] // 4
    [FARule(@"0|[1-9][0-9]*")] // 5
    [FARule(@";")]
    [FARule(@"\[")]
    [FARule(@"\]")]
    [FARule(@"\{")]
    [FARule(@"\}")]
    [FARule(@"\:\:")]
    [FARule(@"\:")]
    [FARule(@"\,")]
    [FARule(@"\+")]
    [FARule(@"\-")]
    [FARule(@"=")]
    internal static partial FATextReaderRunner LexRaw(TextReader reader);

    static IEnumerable<FAMatch> Lex(TextReader reader)
    {
        FATextReaderRunner runner = LexRaw(reader);
        var match = runner.NextMatch();
        while(match.SymbolId!=-2)
        {
            if(match.SymbolId==-1 || match.SymbolId>3)
            {
                yield return match;
            }
            match = runner.NextMatch();
        }
    }
    const int identifier = 4;
    const int integer = 5;
    const int semi = 6;
    const int lbracket = 7;
    const int rbracket = 8;
    const int lbrace = 9;
    const int rbrace = 10;
    const int doublecolon = 11;
    const int colon = 12;
    const int comma = 13;
    const int plus = 14;
    const int minus = 15;
    const int equals = 16;

}

