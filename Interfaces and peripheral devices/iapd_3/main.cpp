#include<iostream>
#include<Windows.h>
#include<winbase.h>
#include<powrprof.h>
#include<setupapi.h>
#include<Poclass.h>
#include <devguid.h>

#pragma comment(lib, "PowrProf.lib")
#pragma comment (lib, "Setupapi.lib")
using namespace std;

BATTERY_INFORMATION GetBatteryState()
{
	BATTERY_INFORMATION bb;

	HDEVINFO hdev =
		SetupDiGetClassDevs(&GUID_DEVCLASS_BATTERY,
			0,
			0,
			DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
	if (INVALID_HANDLE_VALUE != hdev)
	{
		SP_DEVICE_INTERFACE_DATA did = { 0 };
		did.cbSize = sizeof(did);

		if (SetupDiEnumDeviceInterfaces(hdev,
			0,
			&GUID_DEVCLASS_BATTERY,
			0,
			&did))
		{
			DWORD cbRequired = 0;

			SetupDiGetDeviceInterfaceDetail(hdev,
				&did,
				0,
				0,
				&cbRequired,
				0);
			if (ERROR_INSUFFICIENT_BUFFER == GetLastError())
			{
				PSP_DEVICE_INTERFACE_DETAIL_DATA pdidd =
					(PSP_DEVICE_INTERFACE_DETAIL_DATA)LocalAlloc(LPTR,
						cbRequired);
				if (pdidd)
				{
					pdidd->cbSize = sizeof(*pdidd);
					if (SetupDiGetDeviceInterfaceDetail(hdev,
						&did,
						pdidd,
						cbRequired,
						&cbRequired,
						0))
					{
						HANDLE hBattery =
							CreateFile(pdidd->DevicePath,
								GENERIC_READ | GENERIC_WRITE,
								FILE_SHARE_READ | FILE_SHARE_WRITE,
								NULL,
								OPEN_EXISTING,
								FILE_ATTRIBUTE_NORMAL,
								NULL);
						if (INVALID_HANDLE_VALUE != hBattery)
						{
							BATTERY_QUERY_INFORMATION bqi = { 0 };

							DWORD dwWait = 0;
							DWORD dwOut;

							if (DeviceIoControl(hBattery,
								IOCTL_BATTERY_QUERY_TAG,
								&dwWait,
								sizeof(dwWait),
								&bqi.BatteryTag,
								sizeof(bqi.BatteryTag),
								&dwOut,
								NULL)
								&& bqi.BatteryTag)
							{
								BATTERY_INFORMATION bi = { 0 };
								bqi.InformationLevel = BatteryInformation;

								if (DeviceIoControl(hBattery,
									IOCTL_BATTERY_QUERY_INFORMATION,
									&bqi,
									sizeof(bqi),
									&bi,
									sizeof(bi),
									&dwOut,
									NULL))
								{
									bb = bi;
								}
							}
						}
					}
				}
			}
		}

	}

	return bb;
}

int main() {
	cout << "Battery type: " << GetBatteryState().Chemistry << endl;
	SYSTEM_POWER_STATUS status;

	if (!GetSystemPowerStatus(&status)) {
		cout << GetLastError() << endl;
		return -1;
	}

	cout << "Power mode: ";
	BYTE powerMode = status.ACLineStatus;
	switch (powerMode)
	{
	case 0: cout << "Discharging" << endl;
		break;
	case 1: cout << "Charging" << endl;
		break;
	default: cout << "Unknown" << endl;
		break;
	}

	if (!powerMode) {
		DWORD batteryLifeSeconds = status.BatteryLifeTime;
		cout << "The battery will work - " << batteryLifeSeconds / 3600 <<
			"h:" << batteryLifeSeconds % 3600 / 60 << "min:" << batteryLifeSeconds % 3600 % 60 <<
			"sec" << endl;
	}

	cout << "Battery level: ";
	int life = status.BatteryLifePercent;
	cout << life << "%" << endl;

	cout << "Saving mode: ";
	BYTE energyStatus = status.SystemStatusFlag;
	switch (energyStatus)
	{
	case 0: cout << "Disabled." << endl;
		break;
	case 1: cout << "Enabled." << endl;
		break;
	default: cout << "Connected to power supply" << endl;
		break;
	}


	cout << "Choose mode:\n1-hibernation mode\n2-sleep mode\nelse-exit\n";
	char a;
	cin.clear();
	a = getchar();
	switch (a) {
	case '1': SetSuspendState(TRUE, FALSE, FALSE);
		break;
	case '2': SetSuspendState(FALSE, FALSE, FALSE);
		break;
	}
	return 0;
}