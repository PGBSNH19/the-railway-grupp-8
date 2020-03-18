#pragma once
#include <vector>
#include <string>
using namespace std;








//-----------------------------------------------------------------------------
// Station
//-----------------------------------------------------------------------------
class Station
{
public:
	string m_sName;
	Station(string sName, int x, int y);

	bool bAvilable;
	int X, Y;


	// [ERROR]: Needs FSM for busy station

};






//-----------------------------------------------------------------------------
// TravelPlan
//		ex: [0]G�teborg -> [1]Svensh�gen -> [2]Uddevalla
//-----------------------------------------------------------------------------
class TravelPlan
{
public:
	std::vector<Station> vDestinationList;
	std::vector<string> vDepartureTime;

	//vector<Station>::iterator it;


	Station getStationAt(int i);


	void addDestination(Station& station, string sArivalTime);

};
