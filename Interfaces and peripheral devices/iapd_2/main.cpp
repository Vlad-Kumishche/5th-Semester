//Important: run this program as administrator
#include <iostream>
#include <Windows.h>
#include <WinIoCtl.h>
#include <ntddscsi.h>

using namespace std;

const int GB = 1024;
const int KB = 100;
const int BYTE_SIZE = 8;

string busType[] =
{
	"UNKNOWN",
	"SCSI",
	"ATAPI",
	"ATA",
	"ONE_TREE_NINE_FOUR",
	"SSA",
	"FIBRE",
	"USB",
	"RAID",
	"ISCSI",
	"SAS",
	"SATA",
	"SD",
	"MMC"
	"Virtual",
	"FileBackedVirtual",
	"Spaces",
	"Nvme",
	"SCM",
	"Ufs",
	"Max",
	"MaxReserved"
};

void getDevInfo(HANDLE diskHandle, STORAGE_PROPERTY_QUERY storageProtertyQuery)
{
	STORAGE_DEVICE_DESCRIPTOR* deviceDescriptor = (STORAGE_DEVICE_DESCRIPTOR*)calloc(GB, 1);
	deviceDescriptor->Size = GB;

	if (!DeviceIoControl
	(
		diskHandle,
		IOCTL_STORAGE_QUERY_PROPERTY, // device return request 
		&storageProtertyQuery,	      // data buffer pointer
		sizeof(storageProtertyQuery), // input buffer size
		deviceDescriptor,             // output buffer pointer
		GB,	                          // output buffer size
		NULL,
		0
	))
	{
		printf("%d", GetLastError());
		CloseHandle(diskHandle);
		exit(-1);
	}

	puts("_____________________________________");
	cout << "Model: " << (char*)(deviceDescriptor)+deviceDescriptor->ProductIdOffset << endl;
	puts("_____________________________________");
	cout << "Producer: " << ((char*)(deviceDescriptor)+deviceDescriptor->VendorIdOffset == "" ? (char*)(deviceDescriptor)+deviceDescriptor->VendorIdOffset  : "HGST") << endl;
	puts("_____________________________________");
	cout << "Serial number: " << (char*)(deviceDescriptor)+deviceDescriptor->SerialNumberOffset << endl;
	puts("_____________________________________");
	cout << "Interface type: " << busType[deviceDescriptor->BusType].c_str() << endl;
	puts("_____________________________________");
	cout << "Firmware version: " << (char*)(deviceDescriptor)+deviceDescriptor->ProductRevisionOffset << endl;
	puts("_____________________________________");
}

void getMemoryInfo()
{
	string path;
	_ULARGE_INTEGER diskSpace;
	_ULARGE_INTEGER freeSpace;

	diskSpace.QuadPart = 0;
	freeSpace.QuadPart = 0;

	unsigned long int bitMaskForDisks = GetLogicalDrives(); // bit mask which has disk drive

	for (char var = 'A'; var < 'Z'; var++)
	{
		if ((bitMaskForDisks >> var - 65) & 1)
		{
			path = var;
			path.append(":\\");
			GetDiskFreeSpaceEx
			(
				path.c_str(),
				0,
				&diskSpace,
				&freeSpace
			);
			diskSpace.QuadPart = diskSpace.QuadPart / (GB * GB);
			freeSpace.QuadPart = freeSpace.QuadPart / (GB * GB);

			// define disk type (3 - hard drive)
			UINT diskType = GetDriveType(path.c_str());
			if (diskType == 3)
			{
				cout << "Logical Drive: " << var << endl;
				cout << "Total space: " << diskSpace.QuadPart * 1.0 / 1024 << " Gb" << endl;
				cout << "Free space: " << freeSpace.QuadPart * 1.0 / 1024 << " Gb" << endl;
				cout << "Occupied space: " << (diskSpace.QuadPart - freeSpace.QuadPart) * 1.0 / 1024 << " Gb" << endl;
				puts("_____________________________________");
			}
		}
	}
}

void PioDmaSupportStandarts(HANDLE diskHandle)
{
	UCHAR identifyDataBuffer[512 + sizeof(ATA_PASS_THROUGH_EX)] = { 0 };

	// structure to send ATA command to device
	ATA_PASS_THROUGH_EX& PTE = *(ATA_PASS_THROUGH_EX*)identifyDataBuffer;
	PTE.Length = sizeof(PTE);
	PTE.TimeOutValue = 10; // structure size 
	PTE.DataTransferLength = 512; // buffer size
	PTE.DataBufferOffset = sizeof(ATA_PASS_THROUGH_EX);
	PTE.AtaFlags = ATA_FLAGS_DATA_IN; // flag reading bytes from device

	IDEREGS* ideRegs = (IDEREGS*)PTE.CurrentTaskFile;
	ideRegs->bCommandReg = 0xEC; // send ATA command

	// device request
	if (!DeviceIoControl(
		diskHandle,
		IOCTL_ATA_PASS_THROUGH,
		&PTE,
		sizeof(identifyDataBuffer),
		&PTE,
		sizeof(identifyDataBuffer),
		NULL,
		NULL
	))
	{
		cout << GetLastError() << std::endl;
		return;
	}

	// get pointer on the data array 
	WORD* data = (WORD*)(identifyDataBuffer + sizeof(ATA_PASS_THROUGH_EX));
	short ataSupportByte = data[80];
	int i = 2 * BYTE_SIZE;
	int bitArray[2 * BYTE_SIZE];

	// output DMA regimes 
	unsigned short dmaSupportedBytes = data[63];
	int i2 = 2 * BYTE_SIZE;

	// convert bytes in array of bits 
	while (i2--)
	{
		bitArray[i2] = dmaSupportedBytes & 32768 ? 1 : 0;
		dmaSupportedBytes = dmaSupportedBytes << 1;
	}

	cout << "DMA support: ";
	for (int i = 0; i < 8; i++)
	{
		if (bitArray[i] == 1)
		{
			cout << "DMA" << i;
			if (i != 2) cout << ", ";
		}
	}
	cout << endl;
	puts("_____________________________________");

	// output PIO regimes
	unsigned short pioSupportedBytes = data[64];
	int i3 = 2 * BYTE_SIZE;

	// convert bytes in array of bits 
	while (i3--)
	{
		bitArray[i3] = pioSupportedBytes & 32768 ? 1 : 0;
		pioSupportedBytes = pioSupportedBytes << 1;
	}

	cout << "PIO support: ";
	for (int i = 0; i < 2; i++)
	{
		if (bitArray[i] == 1)
		{
			cout << "PIO" << i + 3;
			if (i != 1) cout << ", ";
		}
	}
	cout << endl;
	puts("_____________________________________");
}

// getting descriptor of VXD file
bool initialize(HANDLE& diskHandle)
{
	diskHandle = CreateFile("\\\\.\\PhysicalDrive0", GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ, NULL, OPEN_EXISTING, NULL, NULL);

	if (diskHandle == INVALID_HANDLE_VALUE)
		return false;

	return true;
}

int main()
{
	STORAGE_PROPERTY_QUERY storagePropertyQuery;									// structure with info about request 
	storagePropertyQuery.PropertyId = _STORAGE_PROPERTY_ID::StorageDeviceProperty;	// flag to get device descriptior
	storagePropertyQuery.QueryType = _STORAGE_QUERY_TYPE::PropertyStandardQuery;	// driver request to return device descriptor

	HANDLE diskHandle;

	if (!initialize(diskHandle))
		return EXIT_FAILURE;

	getDevInfo(diskHandle, storagePropertyQuery);
	getMemoryInfo();
	PioDmaSupportStandarts(diskHandle);

	return 0;
}