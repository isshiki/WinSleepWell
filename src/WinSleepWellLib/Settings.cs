namespace WinSleepWell
{
    public class Settings
    {
        public string MouseDeviceId { get; set; } = string.Empty;
        public string BiometricDeviceId { get; set; } = string.Empty;
        public bool MouseAutoToggle { get; set; } = false;
        public bool BiometricAutoToggle { get; set; } = false;
    }
}
