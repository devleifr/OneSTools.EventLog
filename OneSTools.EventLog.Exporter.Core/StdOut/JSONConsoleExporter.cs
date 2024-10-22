using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OneSTools.EventLog.Exporter.Core;


namespace OneSTools.EventLog.Exporter.Core.StdOut
{
    public class JSONConsoleExporter : IEventLogStorage
    {
        // TODO: ENV variable
        private const string PositionFilePath = "/app/data/eventlog_position.txt";

        public async Task<EventLogPosition> ReadEventLogPositionAsync(CancellationToken cancellationToken = default)
        {
            if (File.Exists(PositionFilePath))
            {
                var lines = await File.ReadAllLinesAsync(PositionFilePath, cancellationToken);
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
                var json = JsonSerializer.Serialize(item);
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

            Directory.CreateDirectory(Path.GetDirectoryName(PositionFilePath));
            await File.WriteAllLinesAsync(PositionFilePath, lines, cancellationToken);
        }

        public void Dispose() {}
    }
}
