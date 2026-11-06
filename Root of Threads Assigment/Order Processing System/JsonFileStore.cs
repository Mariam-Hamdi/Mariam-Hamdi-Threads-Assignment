using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Order_Processing_System
{
    public sealed class JsonFileStore
    {
        private readonly string _dataDirectory;
        private static readonly JsonSerializerOptions _op = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        private static readonly SemaphoreSlim _fileLock = new (1,1);
        public JsonFileStore()
        {
            _dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        }
        public async Task<List<T>> ReadAsync<T>(string filename, CancellationToken ct = default)
        {
            var filepath = Path.Combine(_dataDirectory, filename);
            if (!File.Exists(filepath))
            {
                return new List<T>(); // ----> if not exist return empty list
            }
            try
            {
                await using var stream = File.OpenRead(filepath);
                var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _op, ct);
                return data ?? new List<T>();
            }
            catch (Exception ex)
               when (ex is JsonException || ex is NotSupportedException)
            {
                throw new InvalidOperationException($"Data file corrupted '{filename}'.", ex);
            }
        }
        public async Task WriteAsync<T>(string filename,IEnumerable<T>data,CancellationToken ct=default)
        {
            if(!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
            var filepath = Path.Combine(_dataDirectory, filename);
            var temppath = filepath + ".tmp";

            // --> لو عندنا---> thread A ---> شغال و بعد كدا قابل --->await --->
            // ---> هيروح للthread pool---> وهيوقف شغل على التاسك اللي كان بيعملها
            // --> لو جت thread B ---> هتكمل شغل على -->thread A ---> و هنا هيحصل -->DataCorruption 
            // ---> عشان كدا استخدمنا--->  Semaphorslim && WaitAsync && Release --->هتسمح ان
            // thread --->واحد بس يشتغل و التاني مش هيدخل الا لما اعمل --->release
            await _fileLock.WaitAsync(ct); 

            try
            {
                await using (var stream = File.Create(temppath))
                {
                    await JsonSerializer.SerializeAsync(stream,data,_op,ct);
                }
                if (File.Exists(filepath))
                    File.Delete(filepath);
                File.Move(temppath,filepath);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // ====================== Sync ================
        private readonly Lock _synclock = new();
        public List<T> ReadSync<T>(string filename)
        {
            var filepath = Path.Combine(_dataDirectory, filename);
            if (!File.Exists(filepath))
            {
                return new List<T>(); // ----> if not exist return empty list
            }
            try
            {
                 using var stream = File.OpenRead(filepath);
                var data =  JsonSerializer.Deserialize<List<T>>(stream, _op);
                return data ?? new List<T>();
            }
            catch (Exception ex)
               when (ex is JsonException || ex is NotSupportedException)
            {
                throw new InvalidOperationException($"Data file corrupted '{filename}'.", ex);
            }
        }

        public void WriteSync<T>(string filename, IEnumerable<T> data)
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
            var filepath = Path.Combine(_dataDirectory, filename);
            var temppath = filepath + ".tmp";
            //   avoid more than one thread write the same file
            lock (_synclock)
            {
                try
                {
                    using (var stream = File.Create(temppath))
                    {
                        JsonSerializer.Serialize(stream, data, _op);
                    }
                    if (File.Exists(filepath))
                        File.Delete(filepath);
                    File.Move(temppath, filepath);
                }
                catch (Exception ex)
                {
                    if (File.Exists(temppath))
                        File.Delete(temppath);
                    throw new InvalidOperationException($"Failed to write data file '{filename}'.", ex);
                }
            }
        }

    }
}
