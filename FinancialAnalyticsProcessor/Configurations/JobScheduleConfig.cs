namespace FinancialAnalyticsProcessor.Configurations
{
    public class JobScheduleConfig
    {
        public int IntervalInSeconds { get; set; }
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }
}
