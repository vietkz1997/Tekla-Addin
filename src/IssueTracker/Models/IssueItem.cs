using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace BimCommands.Tekla.IssueTracker.Models
{
    public class IssueItem
    {
        public string Code { get; set; } = "001";
        public string Title { get; set; } = "New Issue";
        public string Description { get; set; } = "";
        public string Location { get; set; } = "";
        public List<int> TeklaIds { get; set; } = new List<int>();

        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed
        public string Severity { get; set; } = "Major"; // Critical, Major, Minor, Info
        public string Assignee { get; set; } = "Modeler";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        public string BeforeImageBase64 { get; set; }
        public string AfterImageBase64 { get; set; }

        [JsonIgnore]
        private Bitmap _beforeImage;

        [JsonIgnore]
        public Bitmap BeforeImage
        {
            get
            {
                if (_beforeImage == null && !string.IsNullOrEmpty(BeforeImageBase64))
                {
                    _beforeImage = Base64ToBitmap(BeforeImageBase64);
                }
                return _beforeImage;
            }
            set
            {
                _beforeImage = value;
                BeforeImageBase64 = BitmapToBase64(value);
            }
        }

        [JsonIgnore]
        private Bitmap _afterImage;

        [JsonIgnore]
        public Bitmap AfterImage
        {
            get
            {
                if (_afterImage == null && !string.IsNullOrEmpty(AfterImageBase64))
                {
                    _afterImage = Base64ToBitmap(AfterImageBase64);
                }
                return _afterImage;
            }
            set
            {
                _afterImage = value;
                AfterImageBase64 = BitmapToBase64(value);
            }
        }

        public static string BitmapToBase64(Bitmap bmp)
        {
            if (bmp == null) return null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch { return null; }
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return new Bitmap(ms);
                }
            }
            catch { return null; }
        }
    }

    public class IssueProject
    {
        public string ProjectName { get; set; } = "Tekla BIM Project";
        public string ModelName { get; set; } = "";
        public string Auditor { get; set; } = "QC Engineer";
        public DateTime ExportDate { get; set; } = DateTime.Now;
        public List<IssueItem> Issues { get; set; } = new List<IssueItem>();
    }
}
