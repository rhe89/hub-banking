using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Integration;
using Banking.Services;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.GoogleApi;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.Repository.Core;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class CsvTransactionsImporterCommand : ServiceBusQueueCommand
{
    private readonly ICsvTransactionsImporter _csvTransactionsImporter;
    private readonly ITransactionService _transactionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CsvTransactionsImporterCommand> _logger;
    private readonly IHubDbRepository _dbRepository;

    public CsvTransactionsImporterCommand(
        ICsvTransactionsImporter csvTransactionsImporter,
        ITransactionService transactionService,
        IConfiguration configuration,
        ILogger<CsvTransactionsImporterCommand> logger,
        IHubDbRepository dbRepository)
    {
        _csvTransactionsImporter = csvTransactionsImporter;
        _transactionService = transactionService;
        _configuration = configuration;
        _logger = logger;
        _dbRepository = dbRepository;
    }
    public override async Task Execute(CancellationToken cancellationToken)
    {
        var fileList = await GoogleDriveService.ListFiles(_configuration);
        
        _logger.LogInformation("Found {Count} files to import", fileList.Files.Count);
        
        var importedFiles = await _dbRepository.GetAsync<CsvImport, CsvImportDto>(new Queryable<CsvImport>());

        foreach (var file in fileList.Files.OrderBy(x => x.Name))
        {
            _logger.LogInformation("Importing file {Name} ({Id})", file.Name, file.Id);
            
            if (importedFiles.Any(importedFile => importedFile.FileName == file.Id))
            {
                _logger.LogInformation("File {Name} ({Id}) is already imported", file.Name, file.Id);
                continue;
            }

            var transactionsToImport = await _csvTransactionsImporter.ImportTransactionsFromCsv(file.Id);

            _logger.LogInformation("Found {Count} transactions to import", transactionsToImport.Count);

            if (transactionsToImport.Count == 0)
            {
                continue;
            }
            
            await _transactionService.AddTransactionsFromFile(file.Name, transactionsToImport);
            
            await _dbRepository.AddAsync<CsvImport, CsvImportDto>(new CsvImportDto
            {
                FileName = file.Id
            });
        }
    }

    public override string Trigger => QueueNames.ImportTransactionsCsv;
}

