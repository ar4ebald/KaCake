using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaCake.ViewModels.Home
{
    public class IndexViewModel
    {
        public object ViewModel { get; set; }

        public List<IndexViewModel> SubTree { get; set; }
    }
}
