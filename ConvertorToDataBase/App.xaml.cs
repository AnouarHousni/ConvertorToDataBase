using Microsoft.Maui.Controls;

namespace ConvertorToDataBase
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
