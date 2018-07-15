using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DialogueSmith.Helper
{
    public class FileHelper
    {
        public static List<string> RecursiveListAllFiles(string path)
        {
            List<string> entries = new List<string>();

            Directory.GetFileSystemEntries(path, "*").ToList().ForEach(entry => {
                if (Directory.Exists(entry)) {
                    RecursiveListAllFiles(entry).ForEach(child => {
                        entries.Add(child);
                    });
                } else {
                    entries.Add(entry);
                }
            });

            return entries;
        }
    }
}
