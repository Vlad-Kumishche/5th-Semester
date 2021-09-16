//Important: run this program as administrator
#pragma comment (lib, "Setupapi.lib")
#include <iomanip>
#include <iostream>
#include <Windows.h>
#include <setupapi.h>
#include <regstr.h>

int main()
{
	HDEVINFO hDevInfo;
	//returns a handle to a device information set that contains requested device information elements for a local computer
	//args (ClassGuid, Enumerator, hwndParent, Flags)
	hDevInfo = SetupDiGetClassDevs(NULL, REGSTR_KEY_PCIENUM, 0, DIGCF_PRESENT | DIGCF_ALLCLASSES);
	 
	//defines a device instance that is a member of a device information set
	SP_DEVINFO_DATA spDevInfoData;
	ZeroMemory(&spDevInfoData, sizeof(SP_DEVINFO_DATA));
	spDevInfoData.cbSize = sizeof(SP_DEVINFO_DATA);
	DWORD i = 0;
	while (SetupDiEnumDeviceInfo(hDevInfo, i, &spDevInfoData))
	{
		const int buff = 100;
		TCHAR deviceID[buff];
		ZeroMemory(&deviceID, sizeof(deviceID));

		// get device ID
		SetupDiGetDeviceInstanceId(hDevInfo, &spDevInfoData, deviceID, sizeof(deviceID), NULL);
		std::string id(deviceID);
		std::cout << "# " << i + 1 << std::endl;
		std::cout << "Vendor ID - " << id.substr(8, 4).c_str() << std::endl;
		std::cout << "Device ID - " << id.substr(17, 4).c_str() << std::endl;
		std::cout << "________________" << std::endl;
		i++;
	}

	return 0;
}