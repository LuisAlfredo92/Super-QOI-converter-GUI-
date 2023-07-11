using System.IO;

namespace GUI.Classes;

public class ImageToConvert
{
    private string _toolTipMessage;

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

    public string ToolTipMessage
    {
        get
        {
            return State switch
            {
                ConversionStateEnum.Correct => Assets.Resources.Correct,
                ConversionStateEnum.Waiting => Assets.Resources.Waiting,
                ConversionStateEnum.Working => Assets.Resources.Working,
                null => Assets.Resources.Null_operation,
                _ => _toolTipMessage
            };
        }
        set => _toolTipMessage = value;
    }
}