namespace AeroexpressBotLibrary
{
    public class Aeroexpress
    {
        int _id;
        string _stationStart;
        string _line;
        DateTime _timeStart;
        string _stationEnd;
        DateTime _timeEnd;
        long _globalId;

        public int ID => _id;
        public string StationStart => _stationStart;
        public string Line => _line;
        public string StationEnd => _stationEnd;
        public DateTime TimeStart => _timeStart;
        public DateTime TimeEnd => _timeEnd;
        public long global_id => _globalId;

        public Aeroexpress(int ID, string StationStart, string Line, DateTime TimeStart, string StationEnd, DateTime TimeEnd, long global_id)
        {
            _id = ID;
            _stationStart = StationStart;
            _line = Line;
            _timeStart = TimeStart;
            _stationEnd = StationEnd;
            _timeEnd = TimeEnd;
            _globalId = global_id;
        }
        public string ToCSV() => $"\"{ID}\";\"{StationStart}\";\"{Line}\";\"{TimeStart}\";\"{StationEnd}\";\"{TimeEnd}\";\"{global_id}\";";
    }
}