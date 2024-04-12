namespace AeroexpressBotLibrary
{
    public class CSVProcessing
    {
        /// <summary>
        /// Метод, считывающий данный из потока в формат CSV.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Коллекция объектов класса Aeroexpress.</returns>
        /// <exception cref="Exception"></exception>
        public List<Aeroexpress> Read(Stream stream)
        {
            var reader = new StreamReader(stream);
            var data = new List<Aeroexpress>();
            try
            {
                string check = "\"ID\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";";
                var fTitle = reader.ReadLine();
                if (fTitle != check) { throw new ArgumentException(); } // Проверяем совпадение заголовка.
                var sTitle = reader.ReadLine(); // Заголовок, но на русском, тоже пропускаем.
                check = "\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";\n";
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    try
                    {
                        List<string> values = new List<string>();
                        string[] CSVline = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < CSVline.Length; i++)
                        {
                            if (CSVline.Length != 7) { throw new ArgumentException(); }
                            int x = CSVline.Length;
                            string value = CSVline[i].Trim('"');
                            values.Add(value);
                        }
                        Aeroexpress aeroexpress = new(int.Parse(values[0]), values[1], values[2], DateTime.Parse(values[3]), values[4], DateTime.Parse(values[5]), long.Parse(values[6]));
                        data.Add(aeroexpress);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); throw new Exception(); }
                }
            }
            catch { throw new Exception(); }
            return data;
            
        }
        /// <summary>
        /// Метод, считывающий данный из потока в формат CSV.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <returns>Коллекция объектов класса Aeroexpress.</returns>
        /// <exception cref="Exception"></exception>
        public Stream Write(List<Aeroexpress> list)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            try
            {
                writer.WriteLine("\"ID\";\"StationStart\";\"Line\";\"TimeStart\";\"StationEnd\";\"TimeEnd\";\"global_id\";");
                writer.WriteLine("\"Локальный идентификатор\";\"Станция отправления\";\"Направление Аэроэкспресс\";\"Время отправления со станции\";\"Конечная станция направления Аэроэкспресс\";\"Время прибытия на конечную станцию направления Аэроэкспресс\";\"global_id\";");
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        writer.WriteLine(item.ToCSV());
                    }
                }
                writer.Flush();
                stream.Position = 0;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); throw new Exception(); }
            return stream;
        }
    }
    }


