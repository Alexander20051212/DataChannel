namespace DataChannel.Models;

/// <summary>
/// Представляет сообщение, передаваемое по CAN-шине.
/// </summary>
public class CanMessage
{
    public string Name { get; set; } = string.Empty;

    // Чем меньше идентификатор CAN, тем выше приоритет сообщения.
    public uint CanId { get; set; }

    // Полный размер передаваемого кадра в битах, включая служебные поля.
    public int SizeBits { get; set; }

    // Период формирования сообщения в миллисекундах.
    public double PeriodMs { get; set; }

    // Максимально допустимое время доставки сообщения в миллисекундах.
    public double DeadlineMs { get; set; }
}