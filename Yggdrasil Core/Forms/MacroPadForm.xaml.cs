// MacroPadForm.xaml.cs in Forms
using System.Windows.Controls;

namespace Yggdrasil_Core.Forms
{
    public partial class MacroPadForm : UserControl
    {
        public MacroPadForm()
        {
            InitializeComponent();
            DataContext = new ViewModels.MacroPadVM();
        }
    }
}