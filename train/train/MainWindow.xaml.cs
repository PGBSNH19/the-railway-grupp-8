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


        public MainWindow()
        {
            InitializeComponent();
            cppmain();
        }

        
        //# include <iostream> 
        //# include <string>
        //# include <thread>
        //# include <chrono>         // std::chrono::seconds
        //# include <mutex>
        //# include <vector>
        //# include "Header.h"
        //# include <math.h>       /* pow */


        //#######################################################################- Clock Section-###############################################################################################################################################//
        static public int CLOCKSPEED() { return 200; }  // MACRO SUBSTITUDE
        //-----------------------------------------------------------------------------
        // Clock
        //	- Needs its own thread
        //-----------------------------------------------------------------------------
        class Clock // Singleton
        {
            /*static*/
            int iMin = 0, iHour = 0;

            public void StartClock()
            {
                //while(iHour != 24)

                while (true)
                {
                    iMin++;
                    if (iMin == 60)
                    {
                        iMin = 0;
                        iHour++;
                    }
                    if (iHour == 24)
                    {
                        iHour = 0;
                    }
                    Thread.Sleep(CLOCKSPEED());
                }
                Console.WriteLine("a day has passed");
            }


            public string GetTime()
            {
                return (iHour.ToString() + ":" + iMin.ToString());
            }
        };

        static Clock clockOBJ = new Clock();



        //⚠ ERROR: Thread takes function pointer, temp fix	⚠		
        void startClockVerbose()
        {
            clockOBJ.StartClock();
        }


        //#######################################################################- Visual Logic Section-###############################################################################################################################################//



        public static int GRIDSIZE_X = 16;		// Substitude macros in C# with function that returns int		
        public static int GRIDSIZE_Y = 16;

        static char[,] cmGrid = new char[GRIDSIZE_X, GRIDSIZE_Y];	// static, remmeber last state




//        int pitagoras(int locOneY, int locTwoY, int locOneX, int locTwoX)
//        {
//            int Y = locOneY - locTwoY;
//            int X = locOneX - locTwoX;

//            return sqrt((pow(X, 2) + pow(Y, 2)));
//        }




        //-----------------------------------------------------------------------------
        // Initilize Frame Buffer, 
        //		fills the matrix with '.'
        //-----------------------------------------------------------------------------
        void initializeFrameBuffer()
        {
            //memset(cmGrid, ' ', (GRIDSIZE_Y * GRIDSIZE_X));
            for (int j = 0; j < 16; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    cmGrid[j, i] = ' ';
                }
            }
        }




        //-----------------------------------------------------------------------------
        // Changes pixel at specified location to specified char
        //-----------------------------------------------------------------------------
        static void SetPixel(int x = 0, int y = 0, char cSymbol = '?')
        {
            cmGrid[x, y] = cSymbol;
        }


        //-----------------------------------------------------------------------------
        // Draws the grid to the conosle, should only be called after all functions have changed their pixels
        //	 r̶u̶n̶ ̶o̶n̶ ̶e̶x̶t̶e̶r̶n̶a̶l̶ ̶t̶h̶r̶e̶a̶d̶ ̶d̶u̶e̶ ̶t̶o̶ ̶i̶n̶f̶i̶n̶i̶t̶e̶ ̶w̶h̶i̶l̶e̶ ̶l̶o̶o̶p̶
        //	 call this function from the others each time something happens. pixels only affect themselvs 
        //-----------------------------------------------------------------------------
        void sendFrameToConsole()
        {
            Console.Clear();
            for (byte i = 0; i < GRIDSIZE_X; i++)  // unsgined chars smaller then int
            {
                for (byte j = 0; j < GRIDSIZE_Y; j++)
                {
                    Console.Write(cmGrid[i, j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine(clockOBJ.GetTime());

            //}
        }


        //-----------------------------------------------------------------------------
        // Preset function call to draw default map  
        //-----------------------------------------------------------------------------
        void loadMapPresetToFrameBuffer()
        {
            SetPixel(8, 1, 'S');
            SetPixel(7, 4, '.');
            SetPixel(7, 11, '.');
            SetPixel(8, 2, '.');
            SetPixel(8, 3, '.');
            SetPixel(8, 12, '.');
            SetPixel(8, 13, '.');
            SetPixel(8, 14, 'S');
            //SetPixel(5, 6, '.');
            //SetPixel(5, 7, '.');
            //SetPixel(5, 8, '.');
            //SetPixel(5, 9, '.');

            SetPixel(6, 5, '.');
            SetPixel(6, 6, '.');
            SetPixel(6, 7, 'S');
            SetPixel(6, 8, '.');
            SetPixel(6, 9, '.');
            SetPixel(6, 10, '.');


            SetPixel(9, 4, '.');
            SetPixel(9, 11, '.');

            SetPixel(10, 5, '.');
            SetPixel(10, 6, '.');
            SetPixel(10, 7, 'S');
            SetPixel(10, 8, '.');
            SetPixel(10, 9, '.');
            SetPixel(10, 10, '.');
            /*
                SetPixel(11, 6, '#');
                SetPixel(11, 7, '#');
                SetPixel(11, 8, '#');
                SetPixel(11, 9, '#');*/
        }



        //#######################################################################- Train Logic Section-###############################################################################################################################################//

        //-----------------------------------------------------------------------------
        // Train
        //-----------------------------------------------------------------------------
        class Train
        {
            int m_iLocationX = 0;
            int m_iLocationY = 0;
            string sCurrentlyAt;    // Verbose?
            char m_cSymbol;

            public Train(int spawnLocX, int spawnLocY, char cTrainsLogo)
            {
                m_iLocationX = spawnLocX;
                m_iLocationY = spawnLocY;
                m_cSymbol = cTrainsLogo;

                SetPixel(m_iLocationX, m_iLocationY, m_cSymbol);
            }


            //. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
            // Finite State Machine
            //. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .

            enum State { moving, stationary };
            State state;


            public void startStateMachine(TravelPlan travelPlan)
            {
                /*static*/
                int i = 0;
                while (true)
                {
                    switch (state)
                    {
                        case State.moving:
                            if (travelPlan.vDepartureTime[i] == clockOBJ.GetTime())
                            {
                                try
                                {

                                    sCurrentlyAt = travelPlan.getStationAt(i).m_sName;              // Each Cycle moves the train along one station, destination path stored in vector
                                    Thread.Sleep(2000);

                                    //pitagoras(travelPlan.getStationAt(i).x, travelPlan.getStationAt(i).y, travelPlan.getStationAt(i+1)x, travelPlan.getStationAt(i+1)y); // |
                                    //// set pixel from mid from pitagoras							 // | Find inbetween keyframe
                                    //std::this_thread::sleep_for(std::chrono::milliseconds(2000));	 // | 

                                    i++;

                                    Console.WriteLine("Left " + sCurrentlyAt);

                                    SetPixel(travelPlan.getStationAt(i).X, travelPlan.getStationAt(i).Y, m_cSymbol);
                                    SetPixel(travelPlan.getStationAt(i - 1).X, travelPlan.getStationAt(i - 1).Y, 'S'); // clean up last place

                                    m_iLocationX = travelPlan.getStationAt(i).X;
                                    m_iLocationY = travelPlan.getStationAt(i).Y;


                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("End Of The Line");
                                    return;
                                    throw;
                                }

                                
                                break;
                            }
                            break;

            case State.stationary:
                            break;
                    }

                    if (travelPlan.vDepartureTime[i] == "END")
                    {
                        Console.WriteLine("End Of The Line");
                        break;
                    }
                }
            }

            void start()
            {
                state = State.moving;
            }

            void stop()
            {
                state = State.stationary;
                Console.WriteLine("Train has been stopped by Mr Carlos, Train might miss its departure time");
            }
            //. . . . . . . . . . . . . . . FSM . End. . . . . . . . . . . . . . . . . . . .

        };

        //-----------------------------------------------------------------------------
        // TravelPlan
        //		ex: [0]Göteborg -> [1]Svenshögen -> [2]Uddevalla
        //-----------------------------------------------------------------------------
        class TravelPlan
        {

            public List<Station> vDestinationList = new List<Station>();
            public List<string> vDepartureTime = new List<string>();


            public Station getStationAt(int i)
            {
                Station station = vDestinationList[i];
                return station;
            }


            public void addDestination(Station station, string sArivalTime)
            {
                vDestinationList.Add(station);
                vDepartureTime.Add(sArivalTime);
            }

        }

        //-----------------------------------------------------------------------------
        // Station
        //			m_ vars moved to header file
        //-----------------------------------------------------------------------------
        class Station
        {
            public string m_sName;
            public bool bAvilable;
            public int X, Y;

            // [ERROR]: Needs FSM for busy station
            public Station(string sName, int x, int y)
            {
                m_sName = sName;
                X = x;
                Y = y;
            }
        };


        Train train = new Train(8, 14, 'O');

        static TravelPlan budgetTrip = new TravelPlan();
        //⚠ ERROR: Should be in main but needs to be global for verbose	⚠		

        void verbouse()
        {
            train.startStateMachine(budgetTrip);
            Console.WriteLine("a train completed its route, Killing thread");
        }



        //-----------------------------------------------------------------------------
        // Main
        //-----------------------------------------------------------------------------
        public int cppmain()
        {


            //thread timer(startClockVerbose); <- C++

            //⚠ ERROR: Uses function pointer, external func requiered due to start function being an internal member clock function, FIX THIS	⚠		
            Thread timer = new Thread(new ThreadStart(startClockVerbose));
            timer.Start();




            initializeFrameBuffer();                    // | ERROR: Should load first of all or they'll overload the trains intial spawn location visuals
            loadMapPresetToFrameBuffer();               // | 


            //thread trainOneThread( verbouse);


            //train.stop();

            Station goteborg = new Station("Goteborg", 8, 14);
            Station svenshogen = new Station("Svenshogen", 6, 7);
            Station uddevalla = new Station("Uddevalla", 8, 1);


            budgetTrip.addDestination(goteborg, "0:20");
            budgetTrip.addDestination(svenshogen, "0:59");
            budgetTrip.addDestination(uddevalla, "END");    // END is a keyword that stops the FSM



            Thread trainOneThread = new Thread(new ThreadStart(verbouse));
            trainOneThread.Start();

            //................................................................................................................................................
            //std::this_thread::sleep_for(std::chrono::milliseconds(1020));	  //| Train Will only have time to reach the second station	before .stop is called
            //train.stop();													  //| Train Will only have time to reach the second station	before .stop is called
            //................................................................................................................................................

            //⚠ ERROR: Just for debuging, dont spam update the frame like this!	⚠		

            while (true)
            {
                sendFrameToConsole();
                Thread.Sleep(1020);
            }

        }

    }
}

