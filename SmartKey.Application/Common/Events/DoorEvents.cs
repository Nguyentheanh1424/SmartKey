namespace SmartKey.Application.Common.Events
{
    public static class DoorEvents
    {
        public const string Unknown = "door.unknown";

        public const string Opened = "door.opened";
        public const string Closed = "door.closed";
        
        public const string Locked = "door.locked";
        public const string Unlocked = "door.unlocked";

        public const string BatteryLow = "door.battery_low";
        
        public const string PasscodeAdded = "door.passcode_added";
        public const string PasscodeRemoved = "door.passcode_removed";

        public const string IccardAdded = "door.iccard_added";
        public const string IccardRemoved = "door.iccard_removed";

        public const string SyncCompleted = "door.sync_completed";
    }

    public static class MethodType
    {
        public const string Notification = "Notification";
    }
}
