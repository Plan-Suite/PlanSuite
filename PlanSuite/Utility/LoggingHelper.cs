namespace PlanSuite.Utility
{
    public class LoggingHelper
    {
        private readonly ILogger<LoggingHelper> m_Logger;
        
        public LoggingHelper(ILogger<LoggingHelper> logger)
        {
            m_Logger = logger;
            m_Logger.LogInformation(1, "LoggingHelper has been constructed");
        }

        public void JustADumbFunctionCall()
        {
            m_Logger.LogInformation("JustADumbFunctionCall has been called");
        }
    }
}
