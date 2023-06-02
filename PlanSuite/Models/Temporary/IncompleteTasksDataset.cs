namespace PlanSuite.Models.Temporary
{
    public class IncompleteTasksDataset
    {
        public class IncompleTask
        {
            public IncompleTask(string column, int count)
            {
                Column = column;
                Count = count;
            }

            public string Column { get; set; }
            public int Count { get; set; }
        }

        public List<IncompleTask> IncompleteTasks { get; set; }
    }
}
