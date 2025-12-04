namespace SpnmnTransposer;

internal sealed class KeyOffsetWriter
{
    private Note lastNote = new Note(NoteNumeric.Underline, 0, 0);
    private bool tied = false;

    private readonly Dictionary<NoteNumeric, int> offsetTable = new();

    public KeyOffsetWriter()
    {
        this.RecordMeasureSplit();
    }

    public string WriteNote(Note note)
    {
        if (tied)
        {
            tied = false;
            if (note == lastNote)
            {
                return (note with { KeyOffsetTimes2 = 0 }).ToString();
            }
        }

        int offset = this.offsetTable[note.Numeric];
        if (note.KeyOffsetTimes2 == offset)
        {
            lastNote = note;
            return (note with { KeyOffsetTimes2 = 0 }).ToString();
        }

        this.lastNote = note;
        this.offsetTable[note.Numeric] = note.KeyOffsetTimes2;

        var result = note.ToString();
        if (note.KeyOffsetTimes2 == 0)
            return $"={result}";
        return result;
    }

    public void RecordTie()
    {
        tied = true;
    }

    public void RecordMeasureSplit()
    {
        offsetTable[NoteNumeric.Do] = 0;
        offsetTable[NoteNumeric.Re] = 0;
        offsetTable[NoteNumeric.Mi] = 0;
        offsetTable[NoteNumeric.Fa] = 0;
        offsetTable[NoteNumeric.So] = 0;
        offsetTable[NoteNumeric.La] = 0;
        offsetTable[NoteNumeric.Ti] = 0;
        offsetTable[NoteNumeric.Eight] = 0;
        offsetTable[NoteNumeric.Nine] = 0;
        offsetTable[NoteNumeric.X] = 0;
        offsetTable[NoteNumeric.Y] = 0;
        offsetTable[NoteNumeric.Z] = 0;
        offsetTable[NoteNumeric.Underline] = 0;
    }
}
