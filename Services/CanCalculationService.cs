using DataChannel.Models;

namespace DataChannel.Services;

/// <summary>
/// Сервис расчёта временных характеристик передачи CAN-сообщений.
/// </summary>
public class CanCalculationService
{
    /// <summary>
    /// Рассчитывает время передачи сообщения по CAN-шине.
    /// </summary>
    public double TransmissionTime(CanMessage message, CanChannel channel)
    {
        if (channel.SpeedBitsPerSecond <= 0)
            throw new ArgumentException("Скорость канала должна быть больше нуля.");

        if (message.SizeBits <= 0)
            throw new ArgumentException("Размер сообщения должен быть больше нуля.");

        // Переводим время передачи из секунд в миллисекунды.
        return (double)message.SizeBits /
               channel.SpeedBitsPerSecond * 1000.0;
    }

    /// <summary>
    /// Рассчитывает полную задержку доставки сообщения.
    /// </summary>
    public double DeliveryDelay(CanMessage message, CanChannel channel)
    {
        // Получаем время непосредственной передачи кадра.
        double transmissionTime = TransmissionTime(message, channel);

        // Учитываем передачу, распространение сигнала и обработку.
        return transmissionTime +
               channel.PropagationDelayMs +
               channel.ProcessingDelayMs;
    }
}
