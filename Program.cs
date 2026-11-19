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
            // Загружаем параметры CAN-шины и сообщения.
            CanChannel channel = csvService.LoadChannel(channelPath);
            List<CanMessage> messages =
                csvService.LoadMessages(messagesPath);

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

            Console.WriteLine("\nРЕЗУЛЬТАТЫ РАСЧЕТА ВРЕМЕННЫХ ХАРАКТЕРИСТИК");
            Console.WriteLine("---------------------------------------------------------------");

            Console.WriteLine(
                $"{"Сообщение",-18} {"CAN ID",-9} {"Передача, мс",-14} {"Доставка, мс"}");

            Console.WriteLine(
                "---------------------------------------------------------------");

            foreach (CanMessage message in messages)
            {
                // Рассчитываем время передачи и полную задержку доставки.
                double transmissionTime =
                    calculationService.TransmissionTime(message, channel);

                double deliveryDelay =
                    calculationService.DeliveryDelay(message, channel);

                Console.WriteLine(
                    $"{message.Name,-18} " +
                    $"0x{message.CanId:X3}   " +
                    $"{transmissionTime,-14:F3} " +
                    $"{deliveryDelay:F3}");
            }

            Console.WriteLine("\nРасчеты успешно завершены.");
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
