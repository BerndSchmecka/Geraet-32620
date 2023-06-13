using NAudio.Wave;

public class Program {

    // every sound has a name
    public static Dictionary<int, string> WavNames = new Dictionary<int, string>()
    {
        {0, "0_zero"},
        {1, "1_one"},
        {2, "2_two"},
        {3, "3_three"},
        {4, "4_four"},
        {5, "5_five"},
        {6, "6_six"},
        {7, "7_seven"},
        {8, "8_eight"},
        {9, "9_nine"},
        {10, "10_attention"},
        {11, "11_oblique"},
        {12, "12_end"},
    };

    public static void Main(string[] args) {
        // For now, roms are stored in a folder called "de" in the same directory as the executable
        // normally, there is one card consisting of 6 roms, but there can be more
        // the roms are named 1.bin, 2.bin, 3.bin, 4.bin, 5.bin, 6.bin
        // they have an address range of 0x4000 to 0xFFFF
        // the first 0x40 bytes are the header, the rest is the actual rom
        // the header is 0x40 bytes long, and contains the following:
        /* The first EPROM (Speech 1) of the primary speech board holds a lookup table in the first 64 bytes (0x40). This means that the first sound sample starts at offset 0x40 (actually at address 0x4040 as the EPROM starts at 0x4000). The table has 20 entries of 3 bytes each, and is 0xFF terminated. Only the first 13 entries of the table are used. The rest is reserved for 'future expansion' and is filled with 0xFF. The first two bytes of each entry contain a pointer to the start of the sample (i.e. the start address) in LSB/MSB order. For example: address [0x4AF8] is stored as [0xF8] [0x4A].
        The third byte contains the flags for that entry. The upper nibble of the flags specifies the board that holds the sample. Only bits 6 and 7 of the upper nibble are used. Of the lower nibble, bits 0-2 specify the audio level in the range 0-7 [3]. The remainings bits (3-5) are unused (all 0). */

        // read the roms into memory
        var roms = new List<byte[]>();
        for (int i = 1; i <= 6; i++) {
            var rom = File.ReadAllBytes($"de/{i}.bin");
            roms.Add(rom);
        }

        // create a byte array to hold the entire rom, with offset 0x4000
        var romSize = 0x4000 + roms.Sum(r => r.Length);
        var romData = new byte[romSize];
        var offset = 0x4000;

        // copy the roms into the romData array
        foreach (var rom in roms) {
            Array.Copy(rom, 0, romData, offset, rom.Length);
            offset += rom.Length;
        }

        // access the first 0x40 bytes of the rom (offset 0x4000 to 0x403F) and parse the header
        var header = romData.Skip(0x4000).Take(0x40).ToArray();
        var headerEntries = new List<HeaderEntry>();

        // only the first 13 entries are used
        for (int i = 0; i < 14; i++) {
            var entry = new HeaderEntry();
            entry.Address = BitConverter.ToUInt16(header, i * 3);
            entry.Flags = header[i * 3 + 2];
            headerEntries.Add(entry);

            Console.WriteLine(entry);
        }

        // now we can access the actual sound data
        // each entry contains an address, which is the start of the sound data
        for (int i = 0; i < headerEntries.Count - 1; i++) {
            var address = headerEntries[i].Address;

            // the sound data is until the address of the next entry
            // if the next entry has a flag of 0xFF, then the address of this entry is the end of the sound data

            // read the sound data into a list
            var soundData = new List<byte>();
            while (address < headerEntries[i + 1].Address) {
                soundData.Add(romData[address]);
                address++;
            }


            // convert the list to an array
            var soundDataArray = soundData.ToArray();

            // each sound sample is 8-bit unsigned PCM
            // the sample rate is 8000 Hz
            // the sound data is stored in a format called "IMA ADPCM"

            // convert the sound data to wav
            var wav = new MemoryStream();
            var writer = new WaveFileWriter(wav, new WaveFormat(8000, 8, 1));
            writer.Write(soundDataArray, 0, soundDataArray.Length);
            writer.Close();

            // create a folder called "sounds" in the same directory as the executable (if it doesn't exist)
            Directory.CreateDirectory("sounds");

            // save the wav file
            File.WriteAllBytes($"sounds/{WavNames[i]}.wav", wav.ToArray());
        }
    }
}