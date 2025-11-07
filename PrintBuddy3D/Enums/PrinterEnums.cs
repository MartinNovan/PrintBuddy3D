namespace PrintBuddy3D.Enums;

public class PrinterEnums
{
    public enum Firmware
    {
        Marlin,
        Klipper,
        //OctoPrint, - need to check how to implement it
        //RepRapFirmware, - need more knowledge about it, for now it is not supported
        //mby some other in the future
    }

    public enum Prefix
    {
        Http,
        Https
    }

    public enum Status
    {
        Offline,
        Busy, //Used when printer is doing activity but at the same time it doesn't print
        StandBy, //Used when there is no activity from printer
        Error, //Used when there is error with printer (e.g. When RPI cannot connect to 3D printer board)
        StartUp, //Used when printer is starting
        ShutDown, //Used when u turn off printer or press emergency stop
        //United Statuses
        Printing,
        Complete,
        None, //Used at startup app
    }
}