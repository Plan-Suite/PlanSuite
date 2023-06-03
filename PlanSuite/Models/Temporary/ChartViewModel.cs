using PlanSuite.Models.Persistent;
using System.Drawing;

namespace PlanSuite.Models.Temporary
{
    public class TotalTasksByCompletionStatus
    {
        public class TaskByCompletionStatus
        {
            public TaskByCompletionStatus(string status)
            {
                Status = status;
            }

            public string Status { get; set; }
            public int Count { get; set; }
        }

        public List<TaskByCompletionStatus> TasksByCompletionStatus { get; set; }
    }

    public class ChartViewModel
    {
        public static Color[] ValidColours =
        {
            Color.FromArgb(0x3A, 0x5F, 0xDA),
            Color.FromArgb(0xDA, 0x3A, 0x5F),
            Color.FromArgb(0x5F, 0xDA, 0x3A),
            Color.FromArgb(0x00, 0xA8, 0x96),
            Color.FromArgb(0x96, 0x00, 0xA8),
            Color.FromArgb(0xA8, 0x96, 0x00),
            Color.FromArgb(0xE7, 0x1D, 0x36),
            Color.FromArgb(0x2E, 0x35, 0x32),
            Color.FromArgb(0xFF, 0xFF, 0xFF),
        };

        public Project Project { get; set; }
        public List<ChartDataset> Dataset { get; set; }

        public class ChartDataset
        {
            public ChartDataset(string label, int data, int lastCol)
            {
                Label = label;
                Data = data;

                Colour = ValidColours[lastCol];
            }

            public string Label { get; private set; }
            public int Data { get; private set; }
            public Color Colour { get; private set; }
        }
    }
}
