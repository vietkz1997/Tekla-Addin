using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitQtoTool.Models
{
    /// <summary>
    /// Represents a detected overlap or duplicate issue between two Revit elements.
    /// Implements INotifyPropertyChanged so UI updates when IsJoined/IssueType change after Auto-Join.
    /// </summary>
    public class OverlapIssueItem : INotifyPropertyChanged
    {
        public long ElementIdA { get; set; }
        public string CategoryA { get; set; } = string.Empty;
        public long ElementIdB { get; set; }
        public string CategoryB { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public double ClashingVolumeM3 { get; set; }

        private string _issueType = "Overlap";
        public string IssueType
        {
            get => _issueType;
            set { _issueType = value; OnPropertyChanged(); }
        }

        private bool _isJoined;
        public bool IsJoined
        {
            get => _isJoined;
            set { _isJoined = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
