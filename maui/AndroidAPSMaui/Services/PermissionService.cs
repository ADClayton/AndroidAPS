namespace AndroidAPSMaui.Services;

public class PermissionService
{
    public async Task<bool> EnsureBluetoothPermissionsAsync()
    {
#if ANDROID
        var required = new List<Permissions.BasePermission>
        {
            new Permissions.Bluetooth(),
            //new Permissions.BluetoothLe(),
            new Permissions.LocationWhenInUse()
        };

        foreach (var permission in required)
        {
            var status = await permission.CheckStatusAsync();
            if (status == PermissionStatus.Granted)
            {
                continue;
            }

            status = await permission.RequestAsync();
            if (status != PermissionStatus.Granted)
            {
                return false;
            }
        }
#endif
        return true;
    }
}
