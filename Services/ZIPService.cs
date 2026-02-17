using Chinese_sale_api.DTO;
using Chinese_sale_api.Utilities;
using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Chinese_sale_api.Services
{
    public class ZIPService : IZIPService
    {
        private readonly ILogger<ZIPService> _logger;
        private const string ClassName = nameof(ZIPService);

        public ZIPService(ILogger<ZIPService> logger)
        {
            _logger = logger;
        }

        public void CreateCsvFile(List<GiftWinnerDto> giftWinners, int numLottery, string csvFilePath)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(CreateCsvFile), ClassName, new { FilePath = csvFilePath, LotteryNumber = numLottery });
            try
            {
                using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(giftWinners);
                    csv.WriteComment($"Lottery #{numLottery}\n");
                }
                LoggingHelper.LogCreated(_logger, nameof(CreateCsvFile), ClassName, new { FilePath = csvFilePath, RecordCount = giftWinners.Count });
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(CreateCsvFile), ClassName, ex);
                throw;
            }
        }

        public void CreateZipFile(string csvFilePath, string zipFilePath)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(CreateZipFile), ClassName, new { CsvPath = csvFilePath, ZipPath = zipFilePath });
            try
            {
                using (var zipToCreate = new FileStream(zipFilePath, FileMode.Create))
                using (var archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                {
                    var zipEntry = archive.CreateEntry(Path.GetFileName(csvFilePath), CompressionLevel.Fastest);
                    using (var zipStream = zipEntry.Open())
                    using (var fileStream = new FileStream(csvFilePath, FileMode.Open))
                    {
                        fileStream.CopyTo(zipStream);
                    }
                }
                LoggingHelper.LogCreated(_logger, nameof(CreateZipFile), ClassName, new { ZipPath = zipFilePath });
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(CreateZipFile), ClassName, ex);
                throw;
            }
        }
    }
}
