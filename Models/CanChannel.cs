namespace DataChannel.Models;

/// <summary>
/// Представляет параметры канала передачи данных CAN.
/// </summary>
public class CanChannel
{
    // Скорость передачи данных в битах в секунду.
    public int SpeedBitsPerSecond { get; set; }

    // Задержка распространения сигнала в миллисекундах.
    public double PropagationDelayMs { get; set; }

    // Задержка обработки сообщения в миллисекундах.
    public double ProcessingDelayMs { get; set; }
}