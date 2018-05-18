using System.IO;
using System.Threading.Tasks;

namespace Photon.Library.Variables
{
    public class VariableSetDocument
    {
        private LazyAsync<string> _json;

        public string Id {get; set;}
        public string Filename {get; set;}


        public VariableSetDocument()
        {
            _json = new LazyAsync<string>(LoadJson);
        }

        public VariableSetDocument(string filename) : this()
        {
            this.Filename = filename;
        }

        public async Task<string> GetJson()
        {
            return await _json;
        }

        public void SetJson(string value)
        {
            _json = new LazyAsync<string>(async () => await Task.FromResult(value));
        }

        private async Task<string> LoadJson()
        {
            using (var stream = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) 
            using (var reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }

        internal async Task SaveJson()
        {
            using (var stream = File.Open(Filename, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream)) {
                var json = await _json;
                await writer.WriteAsync(json);
            }
        }
    }
}
