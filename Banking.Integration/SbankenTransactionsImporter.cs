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

public interface ISbankenTransactionsImporter
{
    Task<IList<SbankenTransaction>> ImportTransactionsFromCsv(string fileId);
}

public class SbankenTransactionsImporter : ISbankenTransactionsImporter
{
    private readonly IConfiguration _configuration;

    private static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ";"
    };

    public SbankenTransactionsImporter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IList<SbankenTransaction>> ImportTransactionsFromCsv(string fileId)
    {
        var memoryStreamFile = await GoogleDriveService.DownloadFile(fileId, _configuration);

        var streamReader = new StreamReader(memoryStreamFile);
        using var csv = new CsvReader(streamReader, CsvConfiguration);
        
        csv.Context.RegisterClassMap<SbankenTransactionMap>();
        
        return csv.GetRecords<SbankenTransaction>().ToList();
    }

    [UsedImplicitly]
    private sealed class SbankenTransactionMap : ClassMap<SbankenTransaction>
    {
        public SbankenTransactionMap()
        {
            Map(m => m.AccountingDate).Name("BOKFØRINGSDATO");
            Map(m => m.InterestDate).Name("RENTEDATO");
            Map(m => m.ArchiveReference).Name("ARKIVREFERANSE");
            Map(m => m.RecipientAccountNumber).Name("MOTKONTO");
            Map(m => m.Type).Name("TYPE");
            Map(m => m.Text).Name("TEKST");
            Map(m => m.AmountIn).Name("INN PÅ KONTO").TypeConverter<DecimalConverter>();
            Map(m => m.AmountOut).Name("UT FRA KONTO").TypeConverter<DecimalConverter>();
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