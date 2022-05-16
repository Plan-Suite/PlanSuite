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
            string parsedMd = Markdig.Markdown.ToHtml(markdown);
            Console.WriteLine(parsedMd);
            return parsedMd;
        }
    }

}
