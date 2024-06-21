#region Using directives

using FTOptix.Core;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;

#endregion

public class TrendPensLogic : BaseNetLogic
{
    public override void Start()
    {
        myTrend = Owner.Owner.Owner.Owner.Owner.Get<Trend>("TrendPanel/MainTrend");
        if (myTrend == null)
        {
            Log.Error("TrendRangesLogic", "Cannot get to the main trend, if the widget was tampered, make sure to restore a working path");
            return;
        }
        if (myTrend.Pens.Count == 0)
        {
            // Handle the case where the graph is being generated
            StartupTask = new DelayedTask(StartupLogic, 1000, LogicObject);
            StartupTask.Start();
        }
        else
        {
            // Handle the case where the trend is already in there
            StartupLogic();
        }
    }

    public override void Stop()
    {
        // Stop the pens observer
        referencesEventRegistration?.Dispose();
        referencesObserver = null;
        // Stop the delayed task (if any)
        StartupTask?.Dispose();
    }

    private void StartupLogic()
    {
        // Runtime creation of the different pens data
        if (myTrend.Pens.Count == 0)
        {
            Log.Debug("TrendPensLogic", "No pens to plot");
            return;
        }
        referencesObserver = new ReferencesObserver(myTrend.Pens, LogicObject.Owner.Get<Item>("ScrollView1/Container"));

        referencesEventRegistration = myTrend.Get("Pens").RegisterEventObserver(
            referencesObserver, EventType.ForwardReferenceAdded | EventType.ForwardReferenceRemoved);
    }

    [ExportMethod]
    public void AddPen()
    {
        // Add a new pen at runtime
        var pen = InformationModel.MakeVariable<TrendPen>("Pen" + count, OpcUa.DataTypes.Float);
        var variable = InformationModel.MakeVariable("Variable" + count, OpcUa.DataTypes.Float);
        pen.Color = new Color(255, (byte)randomNumber.Next(0, 255), (byte)randomNumber.Next(0, 255), (byte)randomNumber.Next(0, 255));
        pen.Thickness = 3;
        myTrend.Pens.Add(pen);
        Project.Current.Get("Model/RuntimeAdded").Add(variable);
        pen.SetDynamicLink(variable);
        count++;
    }

    private sealed class ReferencesObserver : IReferenceObserver
    {
        public ReferencesObserver(UAManagedCore.PlaceholderChildNodeCollection<TrendPen> pensNode, Item uiContainer)
        {
            this.uiContainer = uiContainer;
            foreach (var trendPen in pensNode)
            {
                // Create pens that are defined in the DataLogger
                CreatePenUI(trendPen, false);
            }
        }

        public void OnReferenceAdded(IUANode sourceNode, IUANode targetNode, NodeId referenceTypeId, ulong senderId)
        {
            // If a new pen is added at runtime to the trend, we need to add a line
            CreatePenUI(targetNode, true);
        }

        public void OnReferenceRemoved(IUANode sourceNode, IUANode targetNode, NodeId referenceTypeId, ulong senderId)
        {
            // Remove the line if the pen is removed
            var uiThreshold = uiContainer.Get(targetNode.BrowseName);
            if (uiThreshold != null)
                uiThreshold.Delete();
        }

        private void CreatePenUI(IUANode penNode, bool runtimeCreated = false)
        {
            // Add pens to the UI container
            Log.Debug("TrendPensLogic", "Adding " + penNode.BrowseName);
            var penUI = InformationModel.MakeObject<AdvancedTrendPenUI>(penNode.BrowseName);
            penUI.GetVariable("Pen").Value = penNode.NodeId;
            // Check if the pen was added at runtime (it is not in the DataLogger)
            penUI.GetVariable("RuntimeCreated").Value = runtimeCreated;
            uiContainer.Add(penUI);
        }

        private readonly Item uiContainer;
    }

    private Trend myTrend;
    private int count = 0;
    private readonly Random randomNumber = new Random();
    private ReferencesObserver referencesObserver;
    private IEventRegistration referencesEventRegistration;
    private DelayedTask StartupTask;
}
