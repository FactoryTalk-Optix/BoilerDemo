#region Using directives
using FTOptix.Alarm;
using FTOptix.NetLogic;
using FTOptix.UI;
using System.Linq;
using UAManagedCore;
#endregion

public class AlarmsLedLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        NotificationIcon = (Led)Owner;
        alarmCheck = new PeriodicTask(AlarmIconHandler, 500, LogicObject);
        alarmCheck.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        alarmCheck?.Dispose();
    }
    private void AlarmIconHandler()
    {
        IContext context = LogicObject.Context;
        // Get list of alarms from LocalizedAlarms
        var retainedAlarms = context.GetNode(FTOptix.Alarm.Objects.RetainedAlarms);
        var alarmsObjects = ((UAManagedCore.UANode)retainedAlarms).Children?.FirstOrDefault(t => t.BrowseName == "en-US")?.Children;
        // Check for severity
        if (alarmsObjects?.Any() == true)
        {
            NotificationIcon.Visible = true;
            if (alarmsObjects.Any(t => t.GetVariable("Severity").Value >= 100))
            {
                NotificationIcon.Visible = true;
                NotificationIcon.Color = Colors.Red;
            }
            else if (alarmsObjects.Any(t => t.GetVariable("Severity").Value >= 10))
            {
                NotificationIcon.Visible = true;
                NotificationIcon.Color = Colors.Orange;
            }
            else
            {
                NotificationIcon.Visible = true;
                NotificationIcon.Color = Colors.Blue;
            }
        }
        else
        {
            NotificationIcon.Visible = false;
        }
    }

    private PeriodicTask alarmCheck;
    private Led NotificationIcon;
}
