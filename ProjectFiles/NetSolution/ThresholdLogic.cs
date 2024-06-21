#region Using directives

using FTOptix.Core;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using System;
using System.Linq;
using UAManagedCore;

#endregion

public class ThresholdLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        thresholds = LogicObject.GetAlias("Thresholds");
        referencesObserver = new ReferencesObserver(thresholds, LogicObject.Owner.Get<Item>("Accordion1/Content/Container"));

        referencesEventRegistration = thresholds.RegisterEventObserver(
            referencesObserver, EventType.ForwardReferenceAdded | EventType.ForwardReferenceRemoved);
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        referencesEventRegistration?.Dispose();
        referencesObserver = null;
    }

    [ExportMethod]
    public void AddThreshold()
    {
        var threshold = InformationModel.MakeObject<TrendThreshold>("Threshold" + count++);
        threshold.Color = new Color(255, (byte)randomNumber.Next(0, 255), (byte)randomNumber.Next(0, 255), (byte)randomNumber.Next(0, 255));
        threshold.Thickness = 1;
        thresholds.Add(threshold);
    }

    private sealed class ReferencesObserver : IReferenceObserver
    {
        public ReferencesObserver(IUANode thresholdsNode, Item uiContainer)
        {
            this.uiContainer = uiContainer;
            thresholdsNode.Children.ToList().ForEach(CreateThresholdUI);
        }

        public void OnReferenceAdded(IUANode sourceNode, IUANode targetNode, NodeId referenceTypeId, ulong senderId)
        {
            CreateThresholdUI(targetNode);
        }

        public void OnReferenceRemoved(IUANode sourceNode, IUANode targetNode, NodeId referenceTypeId, ulong senderId)
        {
            var uiThreshold = uiContainer.Get(targetNode.BrowseName);
            uiThreshold?.Delete();
        }

        private void CreateThresholdUI(IUANode thresholdNode)
        {
            Log.Debug("ThresholdLogic", "Adding: " + thresholdNode.BrowseName);
            var thresholdUI = InformationModel.MakeObject<AdvancedTrendThresholdUI>(thresholdNode.BrowseName);
            thresholdUI.GetVariable("Threshold").Value = thresholdNode.NodeId;
            uiContainer.Add(thresholdUI);
        }

        private readonly Item uiContainer;
    }

    private int count = 0;
    private IUANode thresholds;
    private readonly Random randomNumber = new Random();
    private ReferencesObserver referencesObserver;
    private IEventRegistration referencesEventRegistration;
}
