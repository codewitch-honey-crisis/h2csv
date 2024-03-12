#nullable disable
#pragma warning disable CS8826 
using System.Text;
using VisualFA;

internal partial class Program
{
    [CmdArg(Ordinal = 0)]
    static TextReader Input { get; set; } = Console.In;
    [CmdArg]
    static FileSystemInfo Output { get; set; } = null;
    [CmdArg(Description = "Generate a header file")]
    static bool Header { get; set; } = false;
       
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
		TextWriter output = null;
		if (Output == null)
		{
			output = Console.Out;
		}
		else
		{
			output = new StreamWriter(File.Open(Output.FullName,FileMode.OpenOrCreate), Encoding.ASCII);
		}
		if (!Header)
        {
            
            foreach (var s in structs)
            {
                if (s.Key == "data_packet" || s.Key == "status_packet" || s.Key == "config_packet")
                {
                    foreach (var f in s.Value)
                    {
                        output.Write("\"" + f.Name + "\", ");
                        output.Write("\"" + f.RawType + "\", ");
                        output.WriteLine(f.RawLength == 0 ? 4 : f.RawLength);
                    }
                    output.WriteLine();
                }
            }
            if(output is StreamWriter)
            {
                output.Close();
            }
            return;

        }
        var sb = new StringBuilder();
        foreach (var s in structs)
        {
            if (s.Key == "data_packet" || s.Key == "status_packet" || s.Key == "config_packet")
            {
                foreach (var f in s.Value)
                {
                    sb.Append("\"" + f.Name + "\", ");
                    sb.Append("\"" + f.RawType + "\", ");
                    sb.AppendLine(Convert.ToString(f.RawLength == 0 ? 4 : f.RawLength));
                }
                sb.AppendLine();
            }
        }
        string hname = "OUTPUT";
        if(output!=Console.Out)
        {
            hname = Path.GetFileNameWithoutExtension(GetFilename(output)).ToUpperInvariant();
        }
        output.WriteLine("#ifndef " + hname + "_H");
        output.WriteLine("#define " + hname + "_H");
        output.Write("#define "+hname+"_CSV ");
        _WriteLiteral(sb.ToString(),output);
        output.WriteLine();
        output.WriteLine("#endif // " + hname + "_H");
		if (output is StreamWriter)
		{
			output.Close();
		}

	}
    static void _WriteLiteral(string input,TextWriter result)
    {
        result.Write("\"");
        int i = 1;
        foreach (var c in input)
        {
            switch (c)
            {
                case '\"': result.Write("\\\""); break;
                case '\\': result.Write(@"\\"); break;
                case '\0': result.Write(@"\0"); break;
                case '\a': result.Write(@"\a"); break;
                case '\b': result.Write(@"\b"); break;
                case '\f': result.Write(@"\f"); break;
                case '\n': result.Write(@"\n"); break;
                case '\r': result.Write(@"\r"); break;
                case '\t': result.Write(@"\t"); break;
                case '\v': result.Write(@"\v"); break;
                default:
                    // ASCII printable character
                    if (c >= 0x20 && c <= 0x7e)
                    {
                        result.Write(c);
                        ++i;
                        // As UTF16 escaped character
                    }
                    else
                    {
                        result.Write(@"\x");
                        result.Write(((int)c).ToString("x2"));
                        i += 4;
                    }
                    break;
            }
            if(i>40)
            {
                i = 0;
                result.WriteLine(@"\");
            }
        }
        result.Write("\"");
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

#pragma warning restore