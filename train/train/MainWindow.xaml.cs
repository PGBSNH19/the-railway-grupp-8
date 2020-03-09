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


using System.Threading;

namespace train
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public static void log(string sString)
        {
            Console.WriteLine( "[" + sString + "]");
        }




        //-----------------------------------------------------------------------------
        // Train
        //-----------------------------------------------------------------------------				
        class Train
        {
           public string sCurrentlyOn = null;
        }




        //-----------------------------------------------------------------------------
        // Station
        //-----------------------------------------------------------------------------				
        class Station
        {

            public string sStationName = null;

            private static Mutex localStationMutex = new Mutex();


            public string TravelToMe(string sTrainName)
            {
                log("waiting for avilable track...");
                // mutex lock
                localStationMutex.WaitOne(-1);



                log("traveling to " + sStationName);
                log(sTrainName + " arrived at station " + /*nameof(this)*/ sStationName);
             
                return this.sStationName;
            }

            public void Leave(string sTrainName)
            {

                log(sTrainName+ " leaving station...");

                // Release the Mutex.
                localStationMutex.ReleaseMutex();

            }
        }



        public MainWindow()
        {
            InitializeComponent();

            Station göteborg = new Station();
            Train trainOne = new Train();
            Train trainTwo = new Train();

            trainOne.sCurrentlyOn = göteborg.TravelToMe(nameof(trainOne));

            göteborg.Leave(nameof(trainOne));

            Thread thread = new Thread(new ThreadStart(() => trainTwo.sCurrentlyOn = göteborg.TravelToMe(nameof(trainTwo))));
            thread.Start();




        }

    }
}
