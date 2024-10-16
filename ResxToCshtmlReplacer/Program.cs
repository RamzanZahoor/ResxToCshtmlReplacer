using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ResxToCshtmlReplacer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Specify the paths for the .cshtml file and .resx file.
            string cshtmlFilePath = @"SampleView.cshtml";
            string resxFilePath = @"resource.resx";

            // Read the content of the .cshtml file
            string cshtmlContent = File.ReadAllText(cshtmlFilePath);

            // Load and parse the .resx file
            XDocument resxDoc = XDocument.Load(resxFilePath);
            var resxEntries = resxDoc.Descendants("data")
                .Select(data => new
                {
                    Name = data.Attribute("name").Value,
                    Value = data.Element("value").Value
                }).ToList();

            // Regular expression to match text between HTML tags (excluding attributes and scripts)
            var betweenTagsRegex = new Regex(@">(.*?)<", RegexOptions.Compiled);
            int i = 0;
            // Loop through each entry in the .resx file
            foreach (var entry in resxEntries)
            {
                // Use regular expression to find matches between HTML tags
                var matches = betweenTagsRegex.Matches(cshtmlContent);
                
                foreach (Match match in matches)
                {
                    string betweenTagsText = match.Groups[1].Value.Trim();  // Extract text between tags

                    // Perform case-sensitive and whole-word comparison
                    if (betweenTagsText == entry.Value)
                    {
                        i++;
                        string replacement = $"@Localizer[\"{entry.Name}\"]";
                        Console.WriteLine($"Replacing \"{betweenTagsText}\" with {replacement}");
                        //Console.Log($"Replacing \"{betweenTagsText}\" with {replacement}");

                        // Replace the matched text between tags with the Localizer syntax
                        cshtmlContent = cshtmlContent.Replace($">{betweenTagsText}<", $">{replacement}<");
                    }
                }
                
            }
            Console.WriteLine($"Total Keys Replaced: {i}");
            // Save the modified content back to the .cshtml file (or a new file if desired)
            File.WriteAllText(cshtmlFilePath, cshtmlContent);

            Console.WriteLine("Localization replacement completed!");
        }
    }
}
