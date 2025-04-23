using System.Text;
using PERI.SK.Domain.Interfaces;
using UglyToad.PdfPig;

namespace PERI.SK.Infrastructure.Data
{
    public class PdfQueries : IDataQueries
    {
        public async Task<bool> CanConnect(string connectionString)
        {
            var path = Path.GetFullPath(connectionString);

            return File.Exists(path);
        }

        public async Task<string> GetData(string connectionString, string? query = null)
        {
            var path = Path.GetFullPath(connectionString);
            var sb = new StringBuilder();

            using (var document = PdfDocument.Open(path))
            {
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }

            return sb.ToString();
        }

        public Task<string> GetFields(string connectionString, string schema, string table)
        {
            throw new NotImplementedException();
        }
    }
}
