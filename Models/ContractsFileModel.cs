namespace ABCRetail.Models
{
    public class ContractsFileModel
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
