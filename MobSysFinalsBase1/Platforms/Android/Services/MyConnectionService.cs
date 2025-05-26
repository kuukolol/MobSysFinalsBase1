using Android.App;
using Android.Content;
using Android.Telecom;
using Android.Telephony;

namespace MobSysFinalsBase1.Platforms.Android.Services
{
    [Service(
        Exported = true,
        Permission = "android.permission.BIND_TELECOM_CONNECTION_SERVICE"
    )]
    public class MyConnectionService : ConnectionService
    {
        public override Connection OnCreateIncomingConnection(PhoneAccountHandle connectionManagerPhoneAccount, ConnectionRequest request)
        {
            var connection = new MyConnection();

            var intent = new Intent(this, typeof(InCallUIActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);

            connection.SetRinging();
            return connection;
        }

        public override Connection OnCreateOutgoingConnection(PhoneAccountHandle connectionManagerPhoneAccount, ConnectionRequest request)
        {
            var connection = new MyConnection();

            var intent = new Intent(this, typeof(InCallUIActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);

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
            SetDisconnected(new global::Android.Telecom.DisconnectCause((Causes)3));
            Destroy();
        }

    }
}
