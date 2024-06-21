#region Using directives

using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.DataLogger;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using System;
using System.Linq;
using UAManagedCore;

#endregion

public class AdvancedTrendLogic : BaseNetLogic
{
    public override void Start()
    {
        ImportPensFromStore();
    }

    public override void Stop()
    {
        // Nothing to do here
    }

    [ExportMethod]
    public void ImportPensFromStore()
    {
        // Insert code to be executed by the method
        var myTrend = Owner.Get<Trend>("TrendPanel/MainTrend");
        if (myTrend == null)
        {
            Log.Error("AdvancedTrendLogic", "Cannot get to the main trend, if the widget was tampered, make sure to restore a working path");
            return;
        }
        // Check if a Store was configured as source
        var sourceLogger = (DataLogger)LogicObject.Context.ResolvePath(myTrend.Get("Model"), myTrend.GetVariable("Model/DynamicLink").Value).ResolvedNode ?? null;
        if (sourceLogger == null)
        {
            Log.Error("AdvancedTrendLogic", "Cannot find a valid logger, make sure you configured the Alias at the widget root");
            return;
        }
        // Get the list of the logged variables
        var loggerVariables = sourceLogger.VariablesToLog.OfType<VariableToLog>();
        if (loggerVariables.ToList().Count < 1)
        {
            Log.Error("AdvancedTrendLogic", "Cannot find any variable in " + sourceLogger.BrowseName);
            return;
        }
        // Remove all existing pens from trend
        myTrend.Pens.Clear();

        foreach (var (srcVar, newPen) in
        // Add the new pens to the trend
        from VariableToLog loggerVariable in loggerVariables
        let newPen = InformationModel.Make<TrendPen>(loggerVariable.BrowseName)
        select (loggerVariable, newPen))
        {
            newPen.Color = new Color(0xFF, (byte)randomNumber.Next(256), (byte)randomNumber.Next(256), (byte)randomNumber.Next(256));
            var dynamicLinkTarget = sourceLogger.GetVariable("VariablesToLog/" + srcVar.BrowseName + "/LastValue");
            newPen.SetDynamicLink(dynamicLinkTarget, DynamicLinkMode.ReadWrite);
            newPen.Thickness = 3;
            myTrend.Pens.Add(newPen);
        }

        Log.Debug("AdvancedTrendLogic", "Pens were added successfully");
    }

    private static readonly Random randomNumber = new Random();
}
