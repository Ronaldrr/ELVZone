using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ELVZone.SplCoverage.Models;
using Newtonsoft.Json;

namespace ELVZone.SplCoverage.Services
{
    public class SpeakerLibraryService
    {
        public IList<SpeakerDefinition> Load(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return new List<SpeakerDefinition>();
            }

            var result = new List<SpeakerDefinition>();
            foreach (var file in Directory.GetFiles(directory, "*.json").OrderBy(path => path))
            {
                var definition = JsonConvert.DeserializeObject<SpeakerDefinition>(File.ReadAllText(file, Encoding.UTF8));
                if (definition == null)
                {
                    continue;
                }

                definition.LibraryFile = file;
                result.Add(definition);
            }

            return result;
        }
    }
}
