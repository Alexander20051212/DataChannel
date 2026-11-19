using DataChannel.Models;
using DataChannel.Services;

namespace DataChannel;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Определяем путь к папке с входными данными.
        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory, "data");

        string channelPath = Path.Combine(
            dataDirectory, "channel.csv");

        string messagesPath = Path.Combine(
            dataDirectory, "messages.csv");

        // Создаем сервисы загрузки и расчета данных.
        var csvService = new CsvDataService();
        var calculationService = new CanCalculationService();

        try
        {
            // Загружаем параметры CAN-шины и список сообщений.
            CanChannel channel = csvService.LoadChannel(channelPath);
            List<CanMessage> messages =
                csvService.LoadMessages(messagesPath);

            // Сортируем сообщения по приоритету CAN ID.
            List<CanMessage> sortedMessages = messages
                .OrderBy(message => message.CanId)
                .ToList();

            // Сохраняем рассчитанные задержки доставки.
            var deliveryDelays = new List<double>();

            Console.WriteLine("==============================================================");
            Console.WriteLine("                  CAN DATA CHANNEL ANALYZER");
            Console.WriteLine("==============================================================");

            Console.WriteLine("\nПАРАМЕТРЫ CAN-ШИНЫ");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine(
                $"Скорость передачи: {channel.SpeedBitsPerSecond / 1000} кбит/с");
            Console.WriteLine(
                $"Задержка распространения: {channel.PropagationDelayMs:F3} мс");
            Console.WriteLine(
                $"Задержка обработки: {channel.ProcessingDelayMs:F3} мс");

            Console.WriteLine("\nРЕЗУЛЬТАТЫ РАСЧЕТА И АРБИТРАЖА");
            Console.WriteLine("--------------------------------------------------------------------------------");

            Console.WriteLine(
                $"{"Сообщение",-17} {"CAN ID",-9} {"Ожидание",-11} {"Передача",-11} {"Доставка",-11} {"Дедлайн",-10} {"Результат"}");

            Console.WriteLine(
                "--------------------------------------------------------------------------------");

            foreach (CanMessage message in sortedMessages)
            {
                // Рассчитываем время ожидания из-за более приоритетных сообщений.
                double waitingTime =
                    calculationService.ArbitrationWaitTime(
                        message, sortedMessages, channel);

                // Рассчитываем время передачи сообщения.
                double transmissionTime =
                    calculationService.TransmissionTime(message, channel);

                // Рассчитываем полную задержку доставки.
                double deliveryDelay =
                    calculationService.ArbitrationDeliveryDelay(
                        message, sortedMessages, channel);

                // Сохраняем задержку для последующего расчета джиттера.
                deliveryDelays.Add(deliveryDelay);

                // Проверяем соблюдение временного ограничения.
                bool meetsDeadline =
                    calculationService.MeetsDeadline(
                        message, deliveryDelay);

                string result = meetsDeadline
                    ? "Выполнен"
                    : "Нарушен";

                Console.WriteLine(
                    $"{message.Name,-17} " +
                    $"0x{message.CanId:X3}   " +
                    $"{waitingTime,-11:F3} " +
                    $"{transmissionTime,-11:F3} " +
                    $"{deliveryDelay,-11:F3} " +
                    $"{message.DeadlineMs,-10:F1} " +
                    $"{result}");
            }

            // Рассчитываем джиттер всех сообщений.
            double jitter =
                calculationService.CalculateJitter(deliveryDelays);

            Console.WriteLine("\nАНАЛИЗ ДЖИТТЕРА");
            Console.WriteLine("--------------------------------------------------------------");

            Console.WriteLine(
                $"Минимальная задержка: {deliveryDelays.Min():F3} мс");

            Console.WriteLine(
                $"Максимальная задержка: {deliveryDelays.Max():F3} мс");

            Console.WriteLine(
                $"Джиттер: {jitter:F3} мс");

            // Подсчитываем количество выполненных и нарушенных дедлайнов.
            int successfulMessages = sortedMessages.Count(message =>
            {
                int index = sortedMessages.IndexOf(message);
                return calculationService.MeetsDeadline(
                    message, deliveryDelays[index]);
            });

            int failedMessages = sortedMessages.Count - successfulMessages;

            Console.WriteLine("\nИТОГ ПРОВЕРКИ ДЕДЛАЙНОВ");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"Успешно доставлено: {successfulMessages}");
            Console.WriteLine($"Нарушено дедлайнов: {failedMessages}");

            Console.WriteLine("\nАнализ временных характеристик завершен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nОШИБКА:");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
