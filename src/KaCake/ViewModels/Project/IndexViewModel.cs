using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaCake.ViewModels.Project
{
    public class IndexViewModel
    {
        public string UserName { get; set; }

        public string TaskGroupName { get; set; }
        public string TaskVariantName { get; set; }

        public int SubmissionId { get; set; }
        public DateTime SubmissionTime { get; set; }

        public FileSystemEntry Root { get; set; }

        public bool UserIsTeacher { get; set; }

        public class FileSystemEntry
        {
            public bool IsDirectory { get; set; }
            public string Name { get; set; }
            public ICollection<FileSystemEntry> SubEntries { get; set; }
        }   
    }
}
