using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace nfe_blob_storage
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _folderWatcher;
        private readonly string _inputFolder;
        private readonly string _connectionStorage;
        private readonly string _containerName;
        private string _fileName;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> settings)
        {
            _logger = logger;
            _inputFolder = settings.Value.InputFolder;
            _connectionStorage = settings.Value.ConnectionStorage;
            _containerName = settings.Value.ContainerName;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Starting");

            if (!Directory.Exists(_inputFolder))
            {
                _logger.LogWarning($"Please make sure the InputFolder [{_inputFolder}] exists, then restart the service.");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Binding Events from Input Folder: {_inputFolder}");

            _folderWatcher = new FileSystemWatcher(_inputFolder, "*.PDF")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            _folderWatcher.Created += Input_OnChanged;
            _folderWatcher.EnableRaisingEvents = true;

            return base.StartAsync(cancellationToken);
        }

        protected void Input_OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created && e.Name != _fileName)
            {
                _logger.LogInformation($"InBound Change Event Triggered by [{e.FullPath}]");

                _fileName = e.Name;

                var blobServiceClient = new BlobServiceClient(_connectionStorage);

                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                var blobClient = containerClient.GetBlobClient(e.Name);

                try
                {
                    using (var fileStream = File.OpenRead(e.FullPath))
                    {
                        blobClient.Upload(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error - Filename: {e.Name} - Message: {ex.Message}");
                }

                _logger.LogInformation("Done with Inbound Change Event");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");

            _folderWatcher.EnableRaisingEvents = false;

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _folderWatcher.Dispose();
            base.Dispose();
        }
    }
}
