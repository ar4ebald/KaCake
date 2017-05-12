using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaCake.ViewModels.Project
{
    public class CommentViewModel
    {
        public int StartRow { get; set; }
        public int StartPosition { get; set; }

        public int EndRow { get; set; }
        public int EndPosition { get; set; }

        public string Text { get; set; }

        public string File { get; set; }
    }
}
