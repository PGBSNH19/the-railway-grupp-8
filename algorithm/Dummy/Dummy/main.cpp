#include <iostream> 
#include <string>
#include <thread>
#include <chrono>         // std::chrono::seconds
#include <mutex>
#include <vector>
#include "Header.h"
#include <math.h>       /* pow */

using namespace std;

//#######################################################################- Clock Section-###############################################################################################################################################//
#define CLOCKSPEED 200
//-----------------------------------------------------------------------------
// Clock
//	- Needs its own thread
//-----------------------------------------------------------------------------
class Clock	// Singleton
{
public:
	/*static*/ int iMin = 0, iHour = 0;

	void StartClock()
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
			std::this_thread::sleep_for(std::chrono::milliseconds(CLOCKSPEED));
		}
		std::cout << "a day has passed" << std::endl;
	}


	string GetTime()
	{
		return (to_string(iHour) + ":" + to_string(iMin));
	}
};

static Clock clockOBJ;



//⚠ ERROR: Thread takes function pointer, temp fix	⚠		
void startClockVerbose()
{
	clockOBJ.StartClock();
}


//#######################################################################- Visual Logic Section-###############################################################################################################################################//



#define GRIDSIZE_X 16	//  C# -->   public static int GRIDSIZE_X(){return 16;};		// Substitude macros in C# with function that returns int		
#define GRIDSIZE_Y 16	//  C# -->   public static int GRIDSIZE_Y(){return 16;};
static char caGrid[GRIDSIZE_X][GRIDSIZE_Y];	// static, remmeber last state




int pitagoras(int locOneY, int locTwoY, int locOneX, int locTwoX)
{
	int Y = locOneY - locTwoY;
	int X = locOneX - locTwoX;

	return sqrt((pow(X, 2) + pow(Y, 2)));
}




//-----------------------------------------------------------------------------
// Initilize Frame Buffer, 
//		fills the matrix with '.'
//-----------------------------------------------------------------------------
void initializeFrameBuffer()
{
	memset(caGrid, ' ', (GRIDSIZE_Y * GRIDSIZE_X));
}


//-----------------------------------------------------------------------------
// Changes pixel at specified location to specified char
//-----------------------------------------------------------------------------
void SetPixel(int x = 0, int y = 0, char cSymbol = '?')
{
	caGrid[x][y] = cSymbol;
}


//-----------------------------------------------------------------------------
// Draws the grid to the conosle, should only be called after all functions have changed their pixels
//	 r̶u̶n̶ ̶o̶n̶ ̶e̶x̶t̶e̶r̶n̶a̶l̶ ̶t̶h̶r̶e̶a̶d̶ ̶d̶u̶e̶ ̶t̶o̶ ̶i̶n̶f̶i̶n̶i̶t̶e̶ ̶w̶h̶i̶l̶e̶ ̶l̶o̶o̶p̶
//	 call this function from the others each time something happens. pixels only affect themselvs 
//-----------------------------------------------------------------------------
void sendFrameToConsole()
{
	/*while (true)
	{*/
	system("CLS");
	for (unsigned char i = 0; i < GRIDSIZE_X; i++)	// unsgined chars smaller then int
	{
		for (unsigned char j = 0; j < GRIDSIZE_Y; j++)
		{
			std::cout << caGrid[i][j];
		}
		cout << std::endl;
	}

	std::cout << clockOBJ.GetTime() << std::endl;

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


//-----------------------------------------------------------------------------
// ITravelable
//-----------------------------------------------------------------------------
class ITravelable
{
public:
	bool bAvilable = true;
	string sName;
};



//-----------------------------------------------------------------------------
// Train
//-----------------------------------------------------------------------------
class Train
{
public:

	/*
		mutex m_mutex;

		int iLocationX = 0;
		int iLocationY = 0;
		string sCurrentlyAt;


		void test()
		{
			for (int i = 0; i < 100; i++)
			{
				std::cout << i << std::endl;
				std::this_thread::sleep_for(std::chrono::milliseconds(25));
			}

		}

		void stop()
		{
			m_mutex.lock();
		}


	*/


	int m_iLocationX = 0;
	int m_iLocationY = 0;
	string sCurrentlyAt;	// Verbose?
	char m_cSymbol;

	Train(int spawnLocX, int spawnLocY, char cTrainsLogo)
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


	void startStateMachine(TravelPlan& travelPlan)
	{
		/*static*/ int i = 0;
		while (true)
		{
			switch (state)
			{
			case moving:

				if (travelPlan.vDepartureTime[i] == clockOBJ.GetTime())
				{
					try
					{
						sCurrentlyAt = travelPlan.getStationAt(i).m_sName;				// Each Cycle moves the train along one station, destination path stored in vector

						std::this_thread::sleep_for(std::chrono::milliseconds(2000));	 // Actual Code		 // Travel Time

						//pitagoras(travelPlan.getStationAt(i).x, travelPlan.getStationAt(i).y, travelPlan.getStationAt(i+1)x, travelPlan.getStationAt(i+1)y); // |
						//// set pixel from mid from pitagoras							 // | Find inbetween keyframe
						//std::this_thread::sleep_for(std::chrono::milliseconds(2000));	 // | 

						i++;

						std::cout << "Left " << sCurrentlyAt << std::endl;

						SetPixel(travelPlan.getStationAt(i).X, travelPlan.getStationAt(i).Y, m_cSymbol);
						SetPixel(travelPlan.getStationAt(i - 1).X, travelPlan.getStationAt(i - 1).Y, 'S'); // clean up last place

						m_iLocationX = travelPlan.getStationAt(i).X;
						m_iLocationY = travelPlan.getStationAt(i).Y;
					}

					catch (const std::exception&)
					{
						std::cout << "End Of The Line" << std::endl;
						return;
					}
					break;
				}

			case stationary:
				break;
			}

			if (travelPlan.vDepartureTime[i] == "END")
			{
				std::cout << "End Of The Line" << std::endl;
				break;
			}
		}
	}

	void start()
	{
		state = moving;
	}

	void stop()
	{
		state = stationary;
		std::cout << "Train has been stopped by Mr Carlos, Train might miss its departure time" << std::endl;
	}

	//. . . . . . . . . . . . . . . FSM . End. . . . . . . . . . . . . . . . . . . .




	//			private static Mutex m_mutex = new Mutex();			
	//			
	//			
	//			int iLocationX = 0;
	//			int iLocationY = 0;
	//			string sCurrentlyAt = null;
	//			
	//			public void test()
	//			{
	//				for (int i = 0; i < 100; i++)
	//				{
	//					Console.WriteLine(i);
	//					Thread.Sleep(100);
	//				}
	//			
	//			}
	//			
	//			
	//			//public bool drive()
	//			//{
	//			//    for (int i = 0; i<vTravelPath.Count; i++)
	//			//    {
	//			//        sCurrentlyAt = vTravelPath[i].sName;
	//			//        // update pixel();
	//			//        Thread.Sleep(100);
	//			//    }
	//			//    return true;
	//			//}
	//			
	//			
	//			
	//			public void stop()
	//			{
	//				m_mutex.WaitOne(-1);
	//			}


};


//-----------------------------------------------------------------------------
// TravelPlan
//		ex: [0]Göteborg -> [1]Svenshögen -> [2]Uddevalla
//			m_ vars moved to header file
//-----------------------------------------------------------------------------
//class TravelPlan
//{
//public:
//	std::vector<Station> vDestinationList;      
//	//vector<Station>::iterator it;
//

Station TravelPlan::getStationAt(int i)
{
	Station station = vDestinationList.at(i);
	return station;
}


void TravelPlan::addDestination(Station& station, string sArivalTime)
{
	vDestinationList.push_back(station);
	vDepartureTime.push_back(sArivalTime);
}

//};



//-----------------------------------------------------------------------------
// Station
//			m_ vars moved to header file
//-----------------------------------------------------------------------------
//class Station : public ITravelable
//{
//public:
	//string m_sName;

Station::Station(string sName, int x, int y)
{
	m_sName = sName;
	X = x;
	Y = y;
}
/*

	bool bAvilable = true;
	int X, Y;*/
	//};



class LevelCrossing : public ITravelable
{
public:
	bool bAvilable = true;
	string sName;
};


Train train(8, 14, 'O');

static TravelPlan budgetTrip;
//⚠ ERROR: Should be in main but needs to be global for verbose	⚠		

void verbouse()
{
	train.startStateMachine(budgetTrip);
	std::cout << "a train completed its route, Killing thread" << std::endl;
}



//-----------------------------------------------------------------------------
// Main
//-----------------------------------------------------------------------------
int main()
{

	thread timer(startClockVerbose);
	//⚠ ERROR: Uses function pointer, external func requiered due to start function being an internal member clock function, FIX THIS	⚠		


	initializeFrameBuffer();					// | ERROR: Should load first of all or they'll overload the trains intial spawn location visuals
	loadMapPresetToFrameBuffer();				// | 


	//thread trainOneThread( verbouse);


	//train.stop();

	Station goteborg("Goteborg", 8, 14), svenshogen("Svenshogen", 6, 7), uddevalla("Uddevalla", 8, 1);


	budgetTrip.addDestination(goteborg, "0:20");
	budgetTrip.addDestination(svenshogen, "0:59");
	budgetTrip.addDestination(uddevalla, "END");	// END is a keyword that stops the FSM

	thread trainOneThread(verbouse);

	//................................................................................................................................................
	//std::this_thread::sleep_for(std::chrono::milliseconds(1020));	  //| Train Will only have time to reach the second station	before .stop is called
	//train.stop();													  //| Train Will only have time to reach the second station	before .stop is called
	//................................................................................................................................................

	//⚠ ERROR: Just for debuging, dont spam update the frame like this!	⚠		

	while (true)
	{
		sendFrameToConsole();
		std::this_thread::sleep_for(std::chrono::milliseconds(1020));	  //| Train Will only have time to reach the second station	before .stop is called
	}

}