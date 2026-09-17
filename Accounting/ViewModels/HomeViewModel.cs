using Accounting.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.ViewModels
{
    public class HomeViewModel : IMenuItem
    {
        public string Title { get; } = "Home";

        //TODO: Add any additional members that will be useful for this screen
    }
}
