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

        // Создаем сервис загрузки данных.
        var csvService = new CsvDataService();

        try
        {
            // Загружаем параметры CAN-шины.
            CanChannel channel = csvService.LoadChannel(channelPath);

            // Загружаем список сообщений.
            List<CanMessage> messages =
                csvService.LoadMessages(messagesPath);

            Console.WriteLine("========================================");
            Console.WriteLine("     CAN DATA CHANNEL ANALYZER");
            Console.WriteLine("========================================");

            Console.WriteLine("\nПАРАМЕТРЫ CAN-ШИНЫ");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine(
                $"Скорость передачи: {channel.SpeedBitsPerSecond / 1000} кбит/с");
            Console.WriteLine(
                $"Задержка распространения: {channel.PropagationDelayMs} мс");
            Console.WriteLine(
                $"Задержка обработки: {channel.ProcessingDelayMs} мс");

            Console.WriteLine("\nЗАГРУЖЕННЫЕ CAN-СООБЩЕНИЯ");
            Console.WriteLine("----------------------------------------");

            Console.WriteLine(
                $"{"Название",-18} {"CAN ID",-10} {"Размер, бит",-13} {"Период, мс",-12} {"Дедлайн, мс"}");

            Console.WriteLine(
                "---------------------------------------------------------------");

            foreach (CanMessage message in messages)
            {
                Console.WriteLine(
                    $"{message.Name,-18} " +
                    $"0x{message.CanId:X3}   " +
                    $"{message.SizeBits,-13} " +
                    $"{message.PeriodMs,-12} " +
                    $"{message.DeadlineMs}");
            }

            Console.WriteLine("\nЗагрузка данных завершена успешно.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nОШИБКА ЗАГРУЗКИ ДАННЫХ:");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
