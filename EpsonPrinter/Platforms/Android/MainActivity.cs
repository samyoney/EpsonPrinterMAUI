using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android;

namespace EpsonPrinter;

[Activity(Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Unspecified)]
public class MainActivity : MauiAppCompatActivity
{
    private const int BLUETOOTH_PERMISSION_REQUEST_CODE = 100;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Platform.Init(this, savedInstanceState);

        Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#2ED573"));
        
        // Request Bluetooth permissions
        RequestBluetoothPermissions();
    }

    private void RequestBluetoothPermissions()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // Android 12+ (API 31+)
        {
            var permissions = new string[]
            {
                Manifest.Permission.BluetoothScan,
                Manifest.Permission.BluetoothConnect
            };
            
            if (!HasPermissions(permissions))
            {
                ActivityCompat.RequestPermissions(this, permissions, BLUETOOTH_PERMISSION_REQUEST_CODE);
            }
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M) // Android 6.0-11 (API 23-30)
        {
            var permissions = new string[]
            {
                Manifest.Permission.Bluetooth,
                Manifest.Permission.BluetoothAdmin,
                Manifest.Permission.AccessFineLocation
            };
            
            if (!HasPermissions(permissions))
            {
                ActivityCompat.RequestPermissions(this, permissions, BLUETOOTH_PERMISSION_REQUEST_CODE);
            }
        }
    }

    private bool HasPermissions(string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
            {
                return false;
            }
        }
        return true;
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        
        if (requestCode == BLUETOOTH_PERMISSION_REQUEST_CODE)
        {
            bool allGranted = true;
            foreach (var result in grantResults)
            {
                if (result != Permission.Granted)
                {
                    allGranted = false;
                    break;
                }
            }
            
            if (allGranted)
            {
                System.Diagnostics.Debug.WriteLine("All Bluetooth permissions granted");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Some Bluetooth permissions denied");
            }
        }
    }
}
