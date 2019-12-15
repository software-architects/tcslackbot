namespace TCSlackbot.Logic.TimeCockpit
{
    public class TCQueryData
    {
        public string Query { get; set; }
        public string Parameters { get; set; }
        public string Expressions { get; set; }
        public string Filters { get; set; }
        public string Condition { get; set; }
        public bool ExpandResult { get; set; }
        public bool Validate { get; set; }

        public TCQueryData(string query)
        {
            Query = query;
        }
    }
}
