#region Using directives
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FTOptix.Alarm;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
#endregion

public class AlarmsLedLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        notificationIcon = (Led)Owner;
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

        if (retainedAlarms == null)
        {
            notificationIcon.Visible = false;
            return;
        }

        // Use regex to find the language folder (e.g., "en-US", "fr-FR")
        var alarmsObjects = ((UAManagedCore.UANode)retainedAlarms).Children?
            .FirstOrDefault(t => isoCodePattern.IsMatch(t.BrowseName))?.Children;

        // Check for severity
        if (alarmsObjects?.Any() == true)
        {
            notificationIcon.Visible = true;
            if (alarmsObjects.Any(t => t.GetVariable("Severity").Value >= 100))
            {
                notificationIcon.Visible = true;
                notificationIcon.Color = Colors.Red;
            }
            else if (alarmsObjects.Any(t => t.GetVariable("Severity").Value >= 10))
            {
                notificationIcon.Visible = true;
                notificationIcon.Color = Colors.Orange;
            }
            else
            {
                notificationIcon.Visible = true;
                notificationIcon.Color = Colors.Blue;
            }
        }
        else
        {
            notificationIcon.Visible = false;
        }
    }

    // Private members
    private PeriodicTask alarmCheck;
    private Led notificationIcon;

    // Pattern for ISO language codes like "en-US", "fr-FR", etc.
    private readonly Regex isoCodePattern = new Regex(@"^[a-z]{2}-[A-Z]{2}$");

}
