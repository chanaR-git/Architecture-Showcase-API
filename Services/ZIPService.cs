using Chinese_sale_api.DTO;
using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Chinese_sale_api.Services
{
    public class ZIPService : IZIPService
    {
        public void CreateCsvFile(List<GiftWinnerDto> giftWinners,int numLottery, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                // כותב את הרשומות (כל הנתונים)
                csv.WriteRecords(giftWinners);
                csv.WriteComment($"Lottery #{numLottery}\n");
            }
        }
        public void CreateZipFile(string csvFilePath, string zipFilePath)
        {
            using (var zipToCreate = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
            {
                // יצירת הערך ה-ZIP לקובץ CSV
                var zipEntry = archive.CreateEntry(Path.GetFileName(csvFilePath), CompressionLevel.Fastest);
                using (var zipStream = zipEntry.Open())
                using (var fileStream = new FileStream(csvFilePath, FileMode.Open))
                {
                    fileStream.CopyTo(zipStream);
                }
            }
        }

    }
}
