using SpnmnTransposer;

var lines = new List<List<NoteAndStringBuilder>>();

{
    Console.Write("File: ");
    var path = Console.ReadLine();

    var origin = File.ReadAllLines(path!);
    var keyOffset = new KeyOffsetReader();
    foreach (var line in origin!)
    {
        var result = new List<NoteAndStringBuilder>();
        lines.Add(result);

        if (!line.TrimStart().StartsWith("N:"))
        {
            result.Add(new NoteAndStringBuilder() { stringBuilder = new(line) });
            continue;
        }

        result.Add(new NoteAndStringBuilder() { stringBuilder = new() });
        
        int squareBracket = 0;
        foreach (var c in line)
        {
            // 忽略所有 [] 里的内容。虽然会导致一些问题，但这个处理起来太复杂了。 
            if (squareBracket > 0)
            {
                switch (c)
                {
                    case '[':
                        squareBracket++;
                        break;
                    case ']':
                        squareBracket--;
                        break;
                }
                result[^1].stringBuilder.Add(c);
                continue;
            }

            switch (c)
            {
                case '|':
                    keyOffset.RecordMeasureSplit();
                    goto default;
                case '~':
                    keyOffset.RecordTie();
                    goto default;
                case '#':
                    keyOffset.RecordAccidental(2);
                    break;
                case 'b':
                    keyOffset.RecordAccidental(2);
                    break;
                case '=':
                    keyOffset.RecordAccidental(0);
                    break;
                case '$':
                    keyOffset.RecordAccidental(1);
                    break;
                case '%':
                    keyOffset.RecordAccidental(-1);
                    break;
                case '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'X' or 'Y' or 'Z' or '_':
                    result.Add(new NoteAndStringBuilder()
                    {
                        note = keyOffset.ReadNote((NoteNumeric)c),
                        stringBuilder = new()
                    });
                    break;
                case 'e':
                {
                    var note = result[^1].note;
                    if (note is not null)
                        result[^1].note = note with { OctaveOffset = note.OctaveOffset + 1 };
                    break;
                }
                case 'd':
                {
                    var note = result[^1].note;
                    if (note is not null)
                        result[^1].note = note with { OctaveOffset = note.OctaveOffset - 1 };
                    break;
                }
                case '[':
                    squareBracket++;
                    goto default;
                default:
                    result[^1].stringBuilder.Add(c);
                    break;
            }
        }
    }
}

var conversion = new Dictionary<Note, Note>();

{
    Console.Write($"Overall transposition (enter '0' to use manual mode): ");
    var transposition = int.Parse(Console.ReadLine()!);

    foreach (var line in lines)
    {
        foreach (var noteAndString in line)
        {
            if (noteAndString.note is null)
                continue;

            if (conversion.ContainsKey(noteAndString.note))
                continue;

            if (transposition is 0)
            {
                Console.Write($"Replace {noteAndString.note} with: ");
                conversion[noteAndString.note] = Note.Parse(Console.ReadLine()!);
            }
            else
            {
                conversion[noteAndString.note] = noteAndString.note.Next(transposition);
                Console.WriteLine($"Replace {noteAndString.note} with: {conversion[noteAndString.note]}");
            }
        }
    }
}

{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();

    var keyOffset = new KeyOffsetWriter();
    foreach (var line in lines)
    {
        foreach (var noteAndString in line)
        {
            if (noteAndString.note is not null)
            {
                Console.Write(keyOffset.WriteNote(conversion[noteAndString.note]));
            }

            foreach (var c in noteAndString.stringBuilder)
            {
                Console.Write(c);
                switch (c)
                {
                    case '|':
                        keyOffset.RecordMeasureSplit();
                        break;
                    case '~':
                        keyOffset.RecordTie();
                        break;
                }
            }
        }
        Console.WriteLine();
    }
}

{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("注意程序可能存在没有考虑的边界情况，需要人工确认。已知问题：");
    Console.WriteLine("一、非 N: 开头的行会保持原样，因此不会处理 Ns");
    Console.WriteLine("二、中括号中的内容会保持原样，因此不会处理倚音");
}