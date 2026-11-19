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

            Console.WriteLine("==============================================");
            Console.WriteLine("          CAN DATA CHANNEL ANALYZER");
            Console.WriteLine("==============================================");

            Console.WriteLine("\nПАРАМЕТРЫ CAN-ШИНЫ");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine(
                $"Скорость передачи: {channel.SpeedBitsPerSecond / 1000} кбит/с");
            Console.WriteLine(
                $"Задержка распространения: {channel.PropagationDelayMs:F3} мс");
            Console.WriteLine(
                $"Задержка обработки: {channel.ProcessingDelayMs:F3} мс");

            Console.WriteLine("\nРЕЗУЛЬТАТЫ ПРИОРИТЕТНОГО АРБИТРАЖА");
            Console.WriteLine("--------------------------------------------------------------------------");

            Console.WriteLine(
                $"{"Сообщение",-17} {"CAN ID",-9} {"Ожидание",-12} {"Передача",-12} {"Доставка",-12}");

            Console.WriteLine(
                "--------------------------------------------------------------------------");

            foreach (CanMessage message in sortedMessages)
            {
                // Рассчитываем время ожидания из-за более приоритетных сообщений.
                double waitingTime =
                    calculationService.ArbitrationWaitTime(
                        message, sortedMessages, channel);

                // Рассчитываем время передачи текущего сообщения.
                double transmissionTime =
                    calculationService.TransmissionTime(message, channel);

                // Рассчитываем полную задержку с учетом арбитража.
                double deliveryDelay =
                    calculationService.ArbitrationDeliveryDelay(
                        message, sortedMessages, channel);

                Console.WriteLine(
                    $"{message.Name,-17} " +
                    $"0x{message.CanId:X3}   " +
                    $"{waitingTime,-12:F3} " +
                    $"{transmissionTime,-12:F3} " +
                    $"{deliveryDelay:F3}");
            }

            Console.WriteLine("\nРасчет приоритетного арбитража завершен.");
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
