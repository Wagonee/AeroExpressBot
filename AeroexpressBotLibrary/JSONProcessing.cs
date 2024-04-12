using System.Text;
using System.Text.Json;

namespace AeroexpressBotLibrary
{
    public class JSONProcessing
    {
        /// <summary>
        /// Метод, считывающий данный из потока в формат JSON.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Коллекция объектов класса Aeroexpress.</returns>
        /// <exception cref="Exception"></exception>
        public List<Aeroexpress> Read(Stream stream)
        {
            var reader = new StreamReader(stream);
            StringBuilder json = new StringBuilder();
            List<Aeroexpress> list = new();
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            try
            {
                while (!reader.EndOfStream)
                {
                    var item = reader.ReadLine();
                    json.Append(item);
                }
                list = JsonSerializer.Deserialize<List<Aeroexpress>>(json.ToString(), options);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); throw new Exception(); }
            return list;
        }
        /// <summary>
        /// Метод возвращающий поток данных для отдачи JSON-файла ботом.
        /// </summary>
        /// <param name="list">Коллекция объектов класса Aeroexpress.</param>
        /// <returns>Поток.</returns>
        /// <exception cref="Exception"></exception>
        public Stream Write(List<Aeroexpress> list)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            try
            {
                string json = JsonSerializer.Serialize(list, options);
                writer.Write(json);

                writer.Flush();
                stream.Position = 0;
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); throw new Exception(); }
            return stream;
        }
    }
}
