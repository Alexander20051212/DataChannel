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
    /// Рассчитывает полную задержку доставки без учета арбитража.
    /// </summary>
    public double DeliveryDelay(CanMessage message, CanChannel channel)
    {
        // Рассчитываем время непосредственной передачи кадра.
        double transmissionTime = TransmissionTime(message, channel);

        // Учитываем передачу, распространение сигнала и обработку.
        return transmissionTime +
               channel.PropagationDelayMs +
               channel.ProcessingDelayMs;
    }

    /// <summary>
    /// Рассчитывает время ожидания из-за более высоких приоритетов.
    /// </summary>
    public double ArbitrationWaitTime(
        CanMessage message,
        List<CanMessage> allMessages,
        CanChannel channel)
    {
        // Находим сообщения с меньшим CAN ID.
        return allMessages
            .Where(otherMessage => otherMessage.CanId < message.CanId)
            .Sum(otherMessage => TransmissionTime(otherMessage, channel));
    }

    /// <summary>
    /// Рассчитывает задержку доставки с учетом приоритетного арбитража.
    /// </summary>
    public double ArbitrationDeliveryDelay(
        CanMessage message,
        List<CanMessage> allMessages,
        CanChannel channel)
    {
        // Определяем время ожидания освобождения CAN-шины.
        double waitingTime = ArbitrationWaitTime(
            message, allMessages, channel);

        // Добавляем время ожидания к обычной задержке доставки.
        return waitingTime + DeliveryDelay(message, channel);
    }

    /// <summary>
    /// Рассчитывает джиттер как разницу между максимальной
    /// и минимальной задержкой доставки.
    /// </summary>
    public double CalculateJitter(List<double> delays)
    {
        if (delays == null || delays.Count < 2)
            return 0;

        // Находим максимальную и минимальную задержки.
        double maxDelay = delays.Max();
        double minDelay = delays.Min();

        // Возвращаем разброс значений задержки.
        return maxDelay - minDelay;
    }

    /// <summary>
    /// Проверяет, укладывается ли сообщение в установленный дедлайн.
    /// </summary>
    public bool MeetsDeadline(CanMessage message, double deliveryDelay)
    {
        // Сравниваем фактическое время доставки с допустимым.
        return deliveryDelay <= message.DeadlineMs;
    }
}
