using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WillDevicesSampleApp.Net
{
    internal class PendoTech
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForConnectDeviceSuccess(string deviceBleMac, string deviceBleName);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForConnectDeviceSuccess([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForConnectDeviceSuccess stProgressCallbackForConnectDeviceSuccess);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForRealTimePenDatas(int x, int y, int pressure);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForRealTimePenDatas([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForRealTimePenDatas stProgressCallbackForRealTimePenDatas);


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForUploadFileStatus(int speed, int percentage, int totalSize);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForUploadFileStatus([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForUploadFileStatus stProgressCallbackForUploadFileStatus);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForUploadFilePenData(int x, int y, int pressure);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForBreakConnected();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForBreakConnected([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForBreakConnected stProgressCallbackForBreakConnected);


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForPluginDevice();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForPluginDevice([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForPluginDevice stProgressCallbackForPluginDevice);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForPulloutDevice();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForPulloutDevice([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForPulloutDevice stProgressCallbackForPulloutDevice);


        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForUploadFilePenData([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForUploadFilePenData stProgressCallbackForUploadFilePenData);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int switchToRealTimeMode();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int switchToUploadMode();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int transferOldestFile();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int queryFileConut(ref int fileConut);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int transferNewestFile();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getUploadFileInfo(ref int fileSize, ref int fileDateTime);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int stopTransfer();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int GetUploadFile();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int deleteUploadFile();

        [DllImport("C:\\Users\\samji\\Downloads\\sdk-for-devices-win-classic-master\\WillDevicesSampleApp\\Pendo\\DigitNoteUSBController.dll")]
        public static extern int connectDevice();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int disconnectDevice();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceDataTimeWithSecond(ref int deviceDataTime);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceName(ref byte deviceName);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceFirmwareVersion(ref byte bleVersion, ref byte mcuVersion);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceBatteryState(ref int batteryPercentage, ref int batteryState);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int setDeviceDateTimeWithSecond(ref int dataTimeWithSeconde);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int setDeviceName(ref byte deviceName);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceMacAddr(ref byte deviceMacAddr);


        [DllImport("DigitNoteUSBController.dll")]
        public static extern int hardwareReset();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int factoryReset();

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int openFileEncryptionWithKey(ref byte key);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int closeFileEncryptionByKey(ref byte key);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int verifyFileEncryptionByKey(ref byte key);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int queryFileEncryptionStatus(ref int FileEncryptStatus);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int scanBinaryCode(ref int iBinaryCode);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceRatio(ref int length, ref int widht);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceDataRange(ref int length, ref int widht, ref int pressureRange);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceBleInfo(ref byte deviceBleMac, ref byte deviceBleName);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int getDeviceSN(ref byte deviceSN);

        [DllImport("DigitNoteUSBController.dll")]
        public static extern int setDeviceSN(ref byte deviceSN);



        [DllImport("DigitNoteUSBController.dll")]
        public static extern void SetCallBackForButton([MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallbackForButton stProgressCallbackForButton);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ProgressCallbackForButton(int buttonValue);


    }
}
