enum Board {
    Board1 = 1,
    Board2 = 2,
    Both = 3
}

class HeaderEntry {
    byte AddressLow;
    byte AddressHigh;

    /* 76543210
        76 = board
            01 = board 1
            10 = board 2
            11 = spread over both boards
        543 = unused/0
        210 = volume (0-7) 
    */
    public byte Flags;

    public ushort Address {
        get {
            return (ushort)(AddressLow + (AddressHigh << 8));
        }
        set {
            AddressLow = (byte)(value & 0xFF);
            AddressHigh = (byte)((value >> 8) & 0xFF);
        }
    }

    public Board Board {
        get {
            return (Board)((Flags >> 6) & 0x03);
        }
        set {
            Flags = (byte)((Flags & 0x3F) | ((byte)value << 6));
        }
    }

    public byte Volume {
        get {
            return (byte)(Flags & 0x07);
        }
        set {
            Flags = (byte)((Flags & 0xF8) | (value & 0x07));
        }
    }

    public override string ToString() {
        return Flags == 0xFF ? $"Address: {Address:X4}, Terminator" : $"Address: {Address:X4}, Board: {Board}, Volume: {Volume}";
    }
}