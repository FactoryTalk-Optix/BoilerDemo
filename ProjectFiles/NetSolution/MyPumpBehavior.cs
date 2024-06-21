#region Using directives
using FTOptix.NetLogic;
using System;
using UAManagedCore;
#endregion

[CustomBehavior]
public class MyPumpBehavior : BaseNetBehavior
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined behavior is started
        quarksCount = new PeriodicTask(PumpLogic, 100, Node);
        quarksCount.Start();
        randomSpeed = new Random();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined behavior is stopped
    }

    [ExportMethod]
    public void RunCw()
    {
        Node.CurrentSpeed = Node.SetSpeed;
        Node.Cw = true;
        Node.Ccw = false;
        Node.Command = true;
        runningTicks = 0;
    }

    [ExportMethod]
    public void RunCcw()
    {
        Node.CurrentSpeed = Node.SetSpeed;
        Node.Ccw = true;
        Node.Cw = false;
        Node.Command = true;
        runningTicks = 0;
    }

    [ExportMethod]
    public void StopMotor()
    {
        Node.CurrentSpeed = 0;
        Node.Command = false;
        Node.Cw = false;
        Node.Ccw = false;
        runningTicks = 0;
        Node.Alarm = false;
    }

    private void PumpLogic()
    {
        if (Node.Cw)
        {
            Node.OutputDatalink = (Node.OutputDatalink + 1) % Node.InputDatalink;
        }
        else if (Node.Ccw)
        {
            if (Node.OutputDatalink > 0)
            {
                Node.OutputDatalink--;
            }
            else
            {
                Node.OutputDatalink = Node.InputDatalink;
            }
        }
        if (Node.Command && !Node.Alarm)
        {
            Node.CurrentSpeed = (float)(randomSpeed.Next((((int)Node.SetSpeed) - 1) * 100, (((int)Node.SetSpeed) + 1) * 100) / 100.0);
        }
        if (!Node.JoggingMode)
        {
            StopMotor();
        }

        runningTicks++;
        if (runningTicks > 100)
        {
            Node.Alarm = true;
            Node.CurrentSpeed = (float)(randomSpeed.Next(5 * 100, 6 * 100) / 100.0);
        }
    }

    PeriodicTask quarksCount;
    Random randomSpeed;
    int runningTicks = 0;

    #region Auto-generated code, do not edit!
    protected new MyPump Node => (MyPump)base.Node;
    #endregion
}
