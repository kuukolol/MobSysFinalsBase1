using Android.App;
using Android.Content;
using Android.OS;
using Android.Telecom;

namespace MobSysFinalsBase1.Platforms.Android.Services
{
    [Service(Exported = true, Permission = "android.permission.BIND_TELECOM_CONNECTION_SERVICE")]
    public class MyConnectionService : ConnectionService
    {
        public override Connection OnCreateIncomingConnection(PhoneAccountHandle connectionManagerPhoneAccount, ConnectionRequest request)
        {
            MyConnection connection = new MyConnection();
            connection.SetRinging();
            return connection;
        }
        public override Connection OnCreateOutgoingConnection(PhoneAccountHandle connectionManagerPhoneAccount, ConnectionRequest request)
        {
            MyConnection connection = new MyConnection();
            connection.SetDialing();
            connection.SetActive();
            return connection;
        }
    }
    public class MyConnection : Connection
    {
        public override void OnAnswer() => SetActive();
        public override void OnDisconnect()
        {
            SetDisconnected(new DisconnectCause((Causes)4));
            Destroy();
        }
    }
}
