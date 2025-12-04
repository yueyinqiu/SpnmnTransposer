using System.Text;

namespace SpnmnTransposer;

internal record class Note(NoteNumeric Numeric, int OctaveOffset, int KeyOffsetTimes2)
{
    private static readonly Dictionary<NoteNumeric, NoteNumeric> numericOrder = new()
    {
        [NoteNumeric.Do] = NoteNumeric.Re,
        [NoteNumeric.Re] = NoteNumeric.Mi,
        [NoteNumeric.Mi] = NoteNumeric.Fa,
        [NoteNumeric.Fa] = NoteNumeric.So,
        [NoteNumeric.So] = NoteNumeric.La,
        [NoteNumeric.La] = NoteNumeric.Ti,
        [NoteNumeric.Ti] = NoteNumeric.Do,
    };
    private static readonly Dictionary<NoteNumeric, NoteNumeric> numericOrderInverse = new()
    {
        [NoteNumeric.Do] = NoteNumeric.Ti,
        [NoteNumeric.Re] = NoteNumeric.Do,
        [NoteNumeric.Mi] = NoteNumeric.Re,
        [NoteNumeric.Fa] = NoteNumeric.Mi,
        [NoteNumeric.So] = NoteNumeric.Fa,
        [NoteNumeric.La] = NoteNumeric.So,
        [NoteNumeric.Ti] = NoteNumeric.La,
    };
    public Note Next(int key)
    {
        if (this.Numeric is not (NoteNumeric.Do or
            NoteNumeric.Re or
            NoteNumeric.Mi or
            NoteNumeric.Fa or
            NoteNumeric.So or
            NoteNumeric.La or
            NoteNumeric.Ti))
        {
            return this;
        }

        var numeric = this.Numeric;
        var octave = this.OctaveOffset;
        var keyOffsetTimes2 = this.KeyOffsetTimes2;

        for (; key > 0; key--)
        {
            switch (numeric)
            {
                case NoteNumeric.Mi:
                    numeric = NoteNumeric.Fa;
                    break;
                case NoteNumeric.Ti:
                    numeric = NoteNumeric.Do;
                    octave++;
                    break;
                default:
                {
                    if (keyOffsetTimes2 < 2)
                    {
                        keyOffsetTimes2 += 2;
                    }
                    else
                    {
                        numeric = numericOrder[numeric];
                        keyOffsetTimes2 -= 2;
                    }
                    break;
                }
            }
        }

        for (; key < 0; key++)
        {
            switch (numeric)
            {
                case NoteNumeric.Fa:
                    numeric = NoteNumeric.Mi;
                    break;
                case NoteNumeric.Do:
                    numeric = NoteNumeric.Ti;
                    octave++;
                    break;
                default:
                {
                    if (keyOffsetTimes2 < 2)
                    {
                        keyOffsetTimes2 -= 2;
                    }
                    else
                    {
                        numeric = numericOrderInverse[numeric];
                        keyOffsetTimes2 += 2;
                    }
                    break;
                }
            }
        }

        return new Note(numeric, octave, keyOffsetTimes2);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        if (KeyOffsetTimes2 > 0)
        {
            _ = builder.Append('#', KeyOffsetTimes2 / 2);
            _ = builder.Append('$', KeyOffsetTimes2 % 2);
        }
        else
        {
            _ = builder.Append('b', -KeyOffsetTimes2 / 2);
            _ = builder.Append('%', -KeyOffsetTimes2 % 2);
        }

        _ = builder.Append((char)Numeric);

        if (OctaveOffset > 0)
            _ = builder.Append('e', OctaveOffset);
        else
            _ = builder.Append('d', -OctaveOffset);

        return builder.ToString();
    }

    public static Note Parse(string s)
    {
        s = s.Trim();

        int keyOffset = 0;
        int i = 0;

        for(; ; i++)
        {
            var change = s[i] switch
            {
                '#' => 2,
                '$' => 1,
                'b' => -2,
                '%' => -1,
                _ => 0
            };
            if (change is 0)
                break;
            keyOffset += change;
        }

        var numeric = (NoteNumeric)s[i];
        i++;

        int octaveOffset = 0;
        for (; i < s.Length; i++)
        {
            var change = s[i] switch
            {
                'e' => 1,
                'd' => -1,
                _ => throw new ArgumentException($"Unexpected character '{s[i]}'.", nameof(s))
            };
            octaveOffset += change;
        }

        return new Note(numeric, octaveOffset, keyOffset);
    }
}
