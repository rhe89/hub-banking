using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Banking.Integration.Dto;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Hub.Shared.GoogleApi;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.Integration;

public interface ISbankenTransactionsImporter
{
    Task<IList<SbankenTransaction>> ImportTransactionsFromCsv(string fileId);
}

public class SbankenTransactionsImporter : ISbankenTransactionsImporter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SbankenTransactionsImporter> _logger;

    private static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ";"
    };

    public SbankenTransactionsImporter(
        IConfiguration configuration,
        ILogger<SbankenTransactionsImporter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IList<SbankenTransaction>> ImportTransactionsFromCsv(string fileId)
    {
        var memoryStreamFile = await GoogleDriveService.DownloadFile(fileId, _configuration);

        var streamReader = new StreamReader(memoryStreamFile);

        var records = new List<SbankenTransaction>();
        
        using var csv = new CsvReader(streamReader, CsvConfiguration);
        
        csv.Context.RegisterClassMap<SbankenTransactionMap>();

        await csv.ReadAsync();
        await csv.ReadAsync();

        csv.ReadHeader();
        
        while (await csv.ReadAsync())
        {
            try
            {
                var record = csv.GetRecord<SbankenTransaction>();
                
                records.Add(record);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading csv");
            }
        }
        

        return records;
    }

    [UsedImplicitly]
    private sealed class SbankenTransactionMap : ClassMap<SbankenTransaction>
    {
        public SbankenTransactionMap()
        {
            Map(m => m.AccountingDate).Index(0);
            Map(m => m.InterestDate).Index(1);
            Map(m => m.ArchiveReference).Index(2);
            Map(m => m.RecipientAccountNumber).Index(3);
            Map(m => m.Type).Index(4);
            Map(m => m.Text).Index(5);
            Map(m => m.AmountOut).Index(6).TypeConverter<DecimalConverter>();
            Map(m => m.AmountIn).Index(7).TypeConverter<DecimalConverter>();
        }
    }
    
    [UsedImplicitly]
    private class DecimalConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            var numberFormatWithComma = new NumberFormatInfo
            {
                NumberDecimalSeparator = ","
            };
            
            return decimal.TryParse(text, NumberStyles.Number, numberFormatWithComma, out var number) ? 
                number : 
                null;
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value.ToString();
        }
    }
}