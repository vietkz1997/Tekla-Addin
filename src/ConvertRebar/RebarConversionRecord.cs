using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BimCommands.ConvertRebar
{
    /// <summary>
    /// Holds details of a rebar item for conversion and rollback (undo).
    /// </summary>
    public class RebarConversionRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string Grade { get; set; } = "";
        public string RebarType { get; set; } = ""; // SingleRebar or RebarGroup
        public int OriginalPointCount { get; set; }
        public int NewPointCount { get; set; }
        public string Status { get; set; } = "Chờ xử lý";
        public Color StatusColor { get; set; } = Color.Gray;
        public Reinforcement ModelObject { get; set; }

        // Backup for Undo
        public List<ArrayList> BackupPolygons { get; set; } = new List<ArrayList>();
        public ArrayList BackupRadiusValues { get; set; }
    }
}
