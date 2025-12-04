using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpnmnTransposer;
internal sealed class KeyOffsetReader
{
    private Note lastNote = new Note(NoteNumeric.Underline, 0, 0);
    private bool tied = false;
    private int? offsetUpdate = null;

    private readonly Dictionary<NoteNumeric, int> offsetTable = new();

    public KeyOffsetReader()
    {
        this.RecordMeasureSplit();
    }

    public Note ReadNote(NoteNumeric note)
    {
        if (offsetUpdate.HasValue)
            this.offsetTable[note] = offsetUpdate.Value;

        int offset = this.offsetTable[note];
        if (tied)
            offset = this.lastNote.KeyOffsetTimes2;

        var result = new Note(note, 0, offset);
        
        tied = false;
        offsetUpdate = null;
        this.lastNote = result;

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

    public void RecordAccidental(int keyOffsetTimes2)
    {
        if (this.offsetUpdate.HasValue)
            this.offsetUpdate = this.offsetUpdate.Value + keyOffsetTimes2;
        else
            this.offsetUpdate = keyOffsetTimes2;
    }
}
