using PrimeTracker.Browsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            //this.vm = new MainWindowViewModel(this);
            //DataContext = vm;

            var ctx = AppContext.InitializeAppContext("E:\\test.db");
            ctx.allVideos.Add(new Models.Video()
            {
                 AmazonId = "1111",
                  Created = DateTime.Now,
                  Updated = DateTime.Now,
                   Description = "tttt",
                    Title = "ttttt",
                     Type = Models.VideoType.Movie,
                      Url = "asdf"
            });
           
            ctx.SaveChanges();

        }




    }
}
