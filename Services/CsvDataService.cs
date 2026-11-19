using System.Globalization;
using DataChannel.Models;

namespace DataChannel.Services;

/// <summary>
/// Сервис для загрузки параметров CAN-шины и сообщений из CSV-файлов.
/// </summary>
public class CsvDataService
{
    /// <summary>
    /// Загружает параметры канала из CSV-файла.
    /// </summary>
    public CanChannel LoadChannel(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Файл параметров канала не найден: {filePath}");

        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
            throw new FormatException("Файл параметров канала пуст.");

        string[] values = lines[1].Split(',');

        if (values.Length < 3)
            throw new FormatException("Некорректный формат файла channel.csv.");

        return new CanChannel
        {
            SpeedBitsPerSecond = int.Parse(
                values[0], CultureInfo.InvariantCulture),

            PropagationDelayMs = double.Parse(
                values[1], CultureInfo.InvariantCulture),

            ProcessingDelayMs = double.Parse(
                values[2], CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Загружает список CAN-сообщений из CSV-файла.
    /// </summary>
    public List<CanMessage> LoadMessages(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Файл сообщений не найден: {filePath}");

        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
            throw new FormatException("Файл сообщений пуст.");

        var messages = new List<CanMessage>();

        // Пропускаем первую строку, содержащую заголовки столбцов.
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = lines[i].Split(',');

            if (values.Length < 5)
                throw new FormatException(
                    $"Некорректные данные в строке {i + 1} файла messages.csv.");

            // Удаляем префикс 0x и преобразуем CAN ID из шестнадцатеричной системы.
            string canIdText = values[1].Trim();

            if (canIdText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                canIdText = canIdText[2..];

            uint canId = uint.Parse(
                canIdText,
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);

            messages.Add(new CanMessage
            {
                Name = values[0].Trim(),
                CanId = canId,
                SizeBits = int.Parse(values[2], CultureInfo.InvariantCulture),
                PeriodMs = double.Parse(values[3], CultureInfo.InvariantCulture),
                DeadlineMs = double.Parse(values[4], CultureInfo.InvariantCulture)
            });
        }

        return messages;
    }
}
