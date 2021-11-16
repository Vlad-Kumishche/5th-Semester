#include <opencv2/opencv.hpp>
#include <Windows.h>
#include <iostream>
#include<setupapi.h>
#include<devguid.h>
#pragma comment(lib, "setupapi.lib")

using namespace cv;

int photoId = 0;
int videoId = 0;
bool isRecording = false;
int isVisible = 0;
HHOOK hook;


void takePhoto();
void keyPressed(int);
LRESULT _stdcall HookCallBack(int nCode, WPARAM wParam, LPARAM lParam);
void record();
void printCameraInfo();

int main() {
	printCameraInfo();

	if (!(hook = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallBack, NULL, 0))) {
		return -1;
	}

	MSG msg;
	while (true) {
		GetMessage(&msg, NULL, 0, 0);
	}

	return 0;
}

void printCameraInfo() {
	setlocale(LC_ALL, "Russian");
	HDEVINFO devInfo = SetupDiGetClassDevsA(&GUID_DEVCLASS_CAMERA, "USB", NULL, DIGCF_PRESENT);
	if (devInfo == INVALID_HANDLE_VALUE) {
		return;
	}

	SP_DEVINFO_DATA devInfoData;
	TCHAR buffer[1024];
	char instanceIDBuffer[1024];

	for (int i = 0; ; i++) {
		devInfoData.cbSize = sizeof(devInfoData);
		if (SetupDiEnumDeviceInfo(devInfo, i, &devInfoData) == FALSE) {
			break;
		}

		memset(buffer, 0, sizeof(buffer));
		SetupDiGetDeviceRegistryProperty(devInfo, &devInfoData, SPDRP_DEVICEDESC, NULL, (BYTE*)buffer, 1024, NULL);
		std::wstring name(buffer);
		memset(buffer, 0, sizeof(buffer));
		SetupDiGetDeviceRegistryProperty(devInfo, &devInfoData, SPDRP_HARDWAREID, NULL, (BYTE*)buffer, 1024, NULL);
		std::wstring ids(buffer);
		std::wstring ven(ids.substr(ids.find(L"VID_") + 4, 4));
		std::wstring dev(ids.substr(ids.find(L"PID_") + 4, 4));
		memset(buffer, 0, sizeof(buffer));
		SetupDiGetDeviceInstanceIdA(devInfo, &devInfoData, (PSTR)instanceIDBuffer, 1024, NULL);
		std::string instanceID(instanceIDBuffer);

		if (name.substr(name.size() - 4, 4) == dev) {
			name = name.substr(0, name.size() - 7);
		}

		std::cout << "Camera Info:" << std::endl;
		std::wcout << "Name: " << name << std::endl;
		std::wcout << "Vendor ID: " << ven << std::endl;
		std::wcout << "Device ID: " << dev << std::endl;
		std::cout << "Instance ID: " << instanceID << std::endl;
		SetupDiDeleteDeviceInfo(devInfo, &devInfoData);
	}
	SetupDiDestroyDeviceInfoList(devInfo);
}

void record() {
	VideoCapture cap(0);
	isRecording = true;

	int frame_width = cap.get(cv::CAP_PROP_FRAME_WIDTH);
	int frame_height = cap.get(cv::CAP_PROP_FRAME_HEIGHT);

	std::string filename = "video" + std::to_string(videoId++) + ".avi";

	VideoWriter video(filename, cv::VideoWriter::fourcc('M', 'J', 'P', 'G'), 10, Size(frame_width, frame_height));
	while (isRecording) {
		Mat frame;

		cap >> frame;

		if (frame.empty())
			break;
		video.write(frame);
		char c = (char)waitKey(1);
		if (c == 27)
			break;
	}

}

void keyPressed(int key) {
	if (key == 1 || key == 2) {
		return;
	}
	switch (key) {
	//p
	case 80: takePhoto();
		break;
	//h
	case 72: {
		ShowWindow(FindWindowA("ConsoleWindowClass", NULL), isVisible);
		if (isVisible == 0) {
			isVisible = 1;
		}
		else {
			isVisible = 0;
		}
	}
		break;
	//v
	case 86: {
		if (isRecording) {
			isRecording = false;
		}
		else {
			record();
		}
	}
		break;
	//b
	case 66: exit(1);
		break;
	default: std::cout << key << std::endl;
		break;
	}
}

void takePhoto() {
	VideoCapture cap(0);

	Mat save_img; cap >> save_img;

	if (save_img.empty())
	{
		std::cerr << "Something is wrong with the webcam, could not get frame." << std::endl;
	}
	std::string filename = "photo" + std::to_string(photoId++) + ".jpg";
	imwrite(filename, save_img);
}

LRESULT _stdcall HookCallBack(int nCode, WPARAM wParam, LPARAM lParam) {
	if (nCode >= 0) {
		if (wParam == WM_KEYDOWN) {
			KBDLLHOOKSTRUCT kbStruct = *((KBDLLHOOKSTRUCT*)lParam);
			keyPressed(kbStruct.vkCode);
		}
	}
	return CallNextHookEx(hook, nCode, wParam, lParam);
}
