using System.Windows.Controls;
using System.Windows.Input;

namespace Accounting.Views.Menu
{
    public class SubItem
    {
        public SubItem(string name,
            UserControl screen = null,
            ICommand command = null)
        {
            Name = name;
            Screen = screen;
            OpenScreenCommand = command;
            

        }
        public string Name { get; private set; }
        public UserControl Screen { get; private set; }
        // Tambahkan Command untuk menangani klik
        public ICommand OpenScreenCommand { get; private set; }

        //public Icon Icon { get; private set; }
    }
}