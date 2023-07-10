using System.IO;

namespace GUI.Classes;

public class ImageToConvert
{
    public ImageToConvert(string filePath, uint id)
    {
        FilePath = filePath;
        State = null;
        FileName = Path.GetFileName(filePath);
        Id = id;
    }

    public uint Id { get; init; }
    public ConversionStateEnum? State { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
}