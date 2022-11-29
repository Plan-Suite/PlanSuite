namespace PlanSuite.Models.Temporary
{
    public class AdminIndexViewModel
    {
        public int UserCount { get; set; }
        public int ProjectCount { get; set; }
        public int CardCount { get; set; }
        public string Section { get; set; }

        public decimal TotalSalesThisMonth { get; set; }
        public decimal PlusSalesThisMonth { get; set; }
        public decimal ProSalesThisMonth { get; set; }

        public decimal TotalSalesLastMonth { get; set; }
        public decimal PlusSalesLastMonth { get; set; }
        public decimal ProSalesLastMonth { get; set; }

        public float UserCountPercentageSinceLastMonth { get; set; }
        public float SalePercentageSinceLastMonth { get; set; }

        public decimal FreeSpace { get; set; }
        public decimal TotalSize { get; set; }

        public float MemoryUsed { get; set; }
        public int TotalTasks { get; set; }
        public int TotalProjects { get; set; }


        decimal CalculateChange(decimal previous, decimal current)
        {
            if (previous == 0)
                return 0;

            if (current == 0)
                return -100;

            var change = ((current - previous) / previous) * 100;

            return change;
        }
    }
}
