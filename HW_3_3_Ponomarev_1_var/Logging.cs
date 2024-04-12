using Serilog;
using Microsoft.Extensions.Logging;

namespace HW_3_3_Ponomarev_1_var
{
    /// <summary>
    /// Класс Logging с единственным полем, позволяющим записывать логи.
    /// </summary>
    public class Logging

    {
        public static ILoggerFactory Logger = LoggerFactory.Create(builder =>
        {
            string relativePath = @"..\..\..\..\var";
            string folderPath = Path.GetFullPath(relativePath);
            DateTime now = DateTime.Now;
            string fileName = now.ToString("yyyyMMdd_HHmmss");
            string fullPath = Path.Combine(folderPath, "log_date_session_" + fileName + ".txt");    
            builder.AddSerilog(new LoggerConfiguration().WriteTo.File(Path.Combine(folderPath, fullPath)).CreateLogger());
            builder.AddConsole();
        });
    }
}
