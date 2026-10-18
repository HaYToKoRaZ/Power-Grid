using System;

namespace PowerGridHaYTooL.Models
{
    public class AutoBackupItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public string CreatedTimeFormatted => CreatedTime.ToString("yyyy-MM-dd HH:mm:ss");
        public long SizeBytes { get; set; }
        public string FileSizeFormatted => $"{SizeBytes / 1024.0:0.#} KB";
        public string PlanCountText { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
