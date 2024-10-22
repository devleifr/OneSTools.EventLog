using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneSTools.EventLog.Exporter.Core;


namespace OneSTools.EventLog.Exporter.Core.StdOut
{
    public class JSONConsoleExporter : IEventLogStorage
    {

        private readonly string _positionFilePath;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JSONConsoleExporter(IConfiguration configuration)
        {
            var configuredPath = configuration.GetValue<string>("Exporter:PositionFilePath");

            _positionFilePath = string.IsNullOrEmpty(configuredPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "eventlog_position.txt")
                : configuredPath;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public async Task<EventLogPosition> ReadEventLogPositionAsync(CancellationToken cancellationToken = default)
        {
            if (File.Exists(_positionFilePath))
            {
                var lines = await File.ReadAllLinesAsync(_positionFilePath, cancellationToken);
                if (lines.Length >= 4)
                {
                    var fileName = lines[0];
                    var endPosition = long.Parse(lines[1]);
                    var lgfEndPosition = long.Parse(lines[2]);
                    var id = long.Parse(lines[3]);

                    return new EventLogPosition(fileName, endPosition, lgfEndPosition, id);
                }
            }

            return null;
        }

        public async Task WriteEventLogDataAsync(List<EventLogItem> items, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                var json = JsonSerializer.Serialize(item, _jsonSerializerOptions);
                Console.WriteLine(json);
            }

            var lastItem = items.Last();
            var lines = new[]
            {
                lastItem.FileName ?? "",
                lastItem.EndPosition.ToString(),
                lastItem.LgfEndPosition.ToString(),
                lastItem.Id.ToString()
            };

            Directory.CreateDirectory(Path.GetDirectoryName(_positionFilePath));
            await File.WriteAllLinesAsync(_positionFilePath, lines, cancellationToken);
        }

        public void Dispose() {}
    }
}
