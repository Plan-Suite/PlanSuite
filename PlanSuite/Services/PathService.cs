using PlanSuite.Interfaces;

namespace PlanSuite.Services
{
    public class PathService : IPathService
    {
        private readonly IWebHostEnvironment m_Environment;
        private ILogger<PathService> m_Logger;

        public PathService(IWebHostEnvironment environment, ILogger<PathService> logger)
        {
            m_Environment = environment;
            m_Logger = logger;
        }

        /// <summary>
        /// Gets the wwwroot path for a specified folder name, also ensures said path actually exists and creates it if it doesnt.
        /// </summary>
        /// <param name="folderName">Path to return</param>
        /// <returns>path_to_wwwroot/folderName</returns>
        public string GetWebRootPath(string folderName)
        {
            m_Logger.LogInformation($"Getting web root path for {folderName}");
            string uploadsFolder = Path.Combine(m_Environment.WebRootPath, "uploaded_images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            m_Logger.LogInformation($"Returned web root path: {uploadsFolder}");
            return uploadsFolder;
        }
    }
}
