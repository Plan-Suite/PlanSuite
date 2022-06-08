using Markdig;
using System.Text.RegularExpressions;

namespace PlanSuite.Utility
{
    public static class Markdown
    {
        /// <summary>
        /// Parse Markdown
        /// </summary>
        /// <param name="markdown">Markdown string to parse</param>
        /// <returns>Markdown string parsed as HTML</returns>
        public static string Parse(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string parsedMd = Markdig.Markdown.ToHtml(markdown, pipeline);

            parsedMd = Regex.Replace(parsedMd, "<\\/?p[^>]*>", string.Empty);

            Console.WriteLine(parsedMd);
            return parsedMd;
        }
    }

}
