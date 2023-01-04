using PlanSuite.Interfaces;

namespace PlanSuite.Services
{
    public class PathService : IPathService
    {
        private readonly IWebHostEnvironment m_Environment;

        public PathService(IWebHostEnvironment environment)
        {
            m_Environment = environment;
        }

        /// <summary>
        /// Gets the wwwroot path for a specified folder name, also ensures said path actually exists and creates it if it doesnt.
        /// </summary>
        /// <param name="folderName">Path to return</param>
        /// <returns>path_to_wwwroot/folderName</returns>
        public string GetWebRootPath(string folderName)
        {
            string uploadsFolder = Path.Combine(m_Environment.WebRootPath, "uploaded_images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            return uploadsFolder;
        }
    }
}
