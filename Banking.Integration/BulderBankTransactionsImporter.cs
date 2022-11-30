using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Banking.Integration.Dto;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Hub.Shared.GoogleApi;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Banking.Integration;

public interface IBulderBankTransactionsImporter
{
    Task<IList<BulderBankTransaction>> ImportTransactionsFromCsv(string fileId);
}

public class BulderBankTransactionsImporter : IBulderBankTransactionsImporter
{
    private readonly IConfiguration _configuration;

    private static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ";"
    };

    public BulderBankTransactionsImporter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IList<BulderBankTransaction>> ImportTransactionsFromCsv(string fileId)
    {
        var memoryStreamFile = await GoogleDriveService.DownloadFile(fileId, _configuration);

        var streamReader = new StreamReader(memoryStreamFile);
        using var csv = new CsvReader(streamReader, CsvConfiguration);
        
        csv.Context.RegisterClassMap<CsvTransactionMap>();
        
        return csv.GetRecords<BulderBankTransaction>().ToList();
    }

    [UsedImplicitly]
    private sealed class CsvTransactionMap : ClassMap<BulderBankTransaction>
    {
        public CsvTransactionMap()
        {
            Map(m => m.TransactionDate).Name("Dato");
            Map(m => m.AmountIn).Name("Inn på konto").TypeConverter<DecimalConverter>();
            Map(m => m.AmountOut).Name("Ut fra konto").TypeConverter<DecimalConverter>();
            Map(m => m.ToAccountName).Name("Til konto");
            Map(m => m.ToAccountNumber).Name("Til kontonummer");
            Map(m => m.FromAccountName).Name("Fra konto");
            Map(m => m.FromAccountNumber).Name("Fra kontonummer");
            Map(m => m.Type).Name("Type");
            Map(m => m.Text).Name("Tekst");
            Map(m => m.Kid).Name("KID");
            Map(m => m.Category).Name("Hovedkategori");
            Map(m => m.SubCategory).Name("Underkategori");
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
                0;
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value.ToString();
        }
    }
}