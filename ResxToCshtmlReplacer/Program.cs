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
            // Specify the paths for the folder containing .cshtml files and the .resx file.
            string cshtmlFolderPath = @"C:\Users\ramzan_zahoor\Downloads\DashboardTest\Dashboard";
            string resxFilePath = @"SharedResource.en.resx";

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

            // Get all .cshtml files in the specified folder
            var cshtmlFiles = Directory.GetFiles(cshtmlFolderPath, "*.cshtml");

            foreach (var cshtmlFilePath in cshtmlFiles)
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(cshtmlFilePath)}");

                // Read the content of each .cshtml file
                string cshtmlContent = File.ReadAllText(cshtmlFilePath);
                int totalReplacements = 0;

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
                            string replacement = $"@Localizer[\"{entry.Name}\"]";
                            Console.WriteLine($"Replacing \"{betweenTagsText}\" with {replacement}");

                            // Replace the matched text between tags with the Localizer syntax
                            cshtmlContent = cshtmlContent.Replace($">{betweenTagsText}<", $">{replacement}<");

                            totalReplacements++;
                        }
                    }
                }

                // Save the modified content back to the same .cshtml file
                File.WriteAllText(cshtmlFilePath, cshtmlContent);
                Console.WriteLine("---------------------------------------------------------------------------");
                Console.WriteLine($"Replaced {totalReplacements} entries in {Path.GetFileName(cshtmlFilePath)}");
                Console.WriteLine("\n\n");
            }

            Console.WriteLine("Localization replacement completed for all files!");
        }
    }
}
