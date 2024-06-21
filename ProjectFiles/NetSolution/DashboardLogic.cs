#region Using directives
using FTOptix.Core;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using System;
using System.Linq;
using UAManagedCore;
#endregion

public class DashboardLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        populateIconsTask = new LongRunningTask(PopulateIcons, LogicObject);
        populateIconsTask.Start();
        populateWidgetsTask = new LongRunningTask(PopulateWidgets, LogicObject);
        populateWidgetsTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        populateIconsTask?.Dispose();
        populateWidgetsTask?.Dispose();
        widgetsPositionObserverTask?.Dispose();
    }

    private void PopulateIcons()
    {
        var iconsContainer = Owner.Get("ElementsContainer");
        IconElement[] iconsToPopulate = new IconElement[6];

        iconsToPopulate[0] = new IconElement
        {
            elementName = "IconGaugeTank1",
            leftMargin = 25,
            topMargin = 30
        };

        iconsToPopulate[1] = new IconElement
        {
            elementName = "IconGaugeTank2",
            leftMargin = 125,
            topMargin = 30
        };

        iconsToPopulate[2] = new IconElement
        {
            elementName = "IconGaugeBoiler",
            leftMargin = 25,
            topMargin = 110
        };

        iconsToPopulate[3] = new IconElement
        {
            elementName = "IconTrendTank1",
            leftMargin = 125,
            topMargin = 110
        };

        iconsToPopulate[4] = new IconElement
        {
            elementName = "IconTrendTank2",
            leftMargin = 25,
            topMargin = 190
        };

        iconsToPopulate[5] = new IconElement
        {
            elementName = "IconTrendBoiler",
            leftMargin = 125,
            topMargin = 190
        };

        foreach (var icon in iconsToPopulate)
        {
            if (iconsContainer.Get(icon.elementName) == null)
            {
                var newWidget = InformationModel.MakeObject(icon.elementName, Project.Current.Get($"UI/Templates/Widgets/Preview/{icon.elementName}").NodeId);
                newWidget.GetVariable("LeftMargin").Value = icon.leftMargin;
                newWidget.GetVariable("TopMargin").Value = icon.topMargin;
                iconsContainer.Add(newWidget);
            }
            else
            {
                var iconElement = iconsContainer.Get(icon.elementName);
                iconElement.GetVariable("LeftMargin").Value = icon.leftMargin;
                iconElement.GetVariable("TopMargin").Value = icon.topMargin;
            }
        }

    }

    private void PopulateWidgets()
    {
        // Stop observer if any
        widgetsPositionObserverTask?.Dispose();
        // Populate dashboard
        var currentUser = InformationModel.Get<User>(Session.User.NodeId);
        var widgetsForThisUser = currentUser.Children.OfType<UserWidget>();
        foreach (var userWidget in widgetsForThisUser)
        {
            if (Owner.Get(userWidget.WidgetBrowseName) == null)
            {
                var newWidget = (Rectangle)InformationModel.MakeObject(userWidget.WidgetBrowseName, userWidget.WidgetType);
                newWidget.LeftMargin = userWidget.WidgetLeftMargin;
                newWidget.TopMargin = userWidget.WidgetTopMargin;
                Owner.Add(newWidget);
            }
        }
        populateWidgetsTask?.Dispose();
        // Restart observer
        if (widgetsPositionObserverTask == null)
        {
            widgetsPositionObserverTask = new PeriodicTask(PositionsObserver, 250, LogicObject);
            widgetsPositionObserverTask.Start();
        }
    }

    private void PositionsObserver()
    {
        var originalElements = Project.Current.Get("UI/Screens/Dashboard").Children;
        var elementsInTypePage = originalElements.Select(attribute => attribute.BrowseName).ToHashSet();
        var elementsInCurrentPage = Owner.Children.Select(attribute => attribute.BrowseName).ToHashSet();
        var widgetsForThisUser = Session.User.Children.OfType<UserWidget>().Select(item => item.WidgetBrowseName).ToHashSet();

        bool newUser = Session.User.NodeId != lastUser;

        if (Session.User.BrowseName == "Anonymous" || newUser)
        {
            // Remove elements not present in the type page
            foreach (var element in elementsInCurrentPage.Except(elementsInTypePage).ToList())
            {
                Owner.Get(element)?.Delete();
            }
            Owner.GetVariable("ElementsContainer/SidePanel").Value = false;
        }
        else
        {
            // Update or add elements for the current user
            foreach (var elementInCurrentPage in elementsInCurrentPage)
            {
                if (!elementsInTypePage.Contains(elementInCurrentPage))
                {
                    if (widgetsForThisUser.Contains(elementInCurrentPage))
                    {
                        var userWidgetToUpdate = Session.User.Children.OfType<UserWidget>().FirstOrDefault(item => item.WidgetBrowseName == elementInCurrentPage);
                        if (userWidgetToUpdate != null)
                        {
                            var rectangle = Owner.Get<Rectangle>(elementInCurrentPage);
                            if (rectangle != null)
                            {
                                userWidgetToUpdate.WidgetTopMargin = rectangle.TopMargin;
                                userWidgetToUpdate.WidgetLeftMargin = rectangle.LeftMargin;
                            }
                        }
                    }
                    else
                    {
                        var newElement = InformationModel.Make<UserWidget>(RandomString());
                        newElement.WidgetBrowseName = elementInCurrentPage;
                        var rectangle = Owner.Get<Rectangle>(elementInCurrentPage);
                        if (rectangle != null)
                        {
                            newElement.WidgetTopMargin = rectangle.TopMargin;
                            newElement.WidgetLeftMargin = rectangle.LeftMargin;
                            newElement.WidgetType = rectangle.ObjectType.NodeId;
                            Session.User.Add(newElement);
                        }
                    }
                }
            }

            // Add missing widgets for the current user
            foreach (var element in widgetsForThisUser.Except(elementsInCurrentPage))
            {
                var elementWidget = Session.User.Children.OfType<UserWidget>().FirstOrDefault(item => item.WidgetBrowseName == element);
                if (elementWidget != null && Owner.Get(elementWidget.WidgetBrowseName) == null)
                {
                    var newWidget = (Rectangle)InformationModel.MakeObject(elementWidget.WidgetBrowseName, elementWidget.WidgetType);
                    newWidget.LeftMargin = elementWidget.WidgetLeftMargin;
                    newWidget.TopMargin = elementWidget.WidgetTopMargin;
                    Owner.Add(newWidget);
                }
            }
        }

        lastUser = Session.User.NodeId;
    }

    [ExportMethod]
    public void WidgetDropped(NodeId widgetIcon)
    {
        var iconWidget = InformationModel.Get<Rectangle>(widgetIcon);
        if (iconWidget.LeftMargin > 220)
        {
            var iconWidgetName = iconWidget.BrowseName;
            var fullWidget = Project.Current.Get<RectangleType>($"UI/Templates/Widgets/Components/{iconWidgetName.Replace("Icon", "Widget")}");
            if (fullWidget != null)
            {
                var newWidget = (Rectangle)InformationModel.MakeObject(RandomString(), fullWidget.NodeId);
                newWidget.LeftMargin = iconWidget.LeftMargin;
                newWidget.TopMargin = iconWidget.TopMargin;
                Owner.Add(newWidget);
            }
            else
            {
                Log.Error("DashboardLogic.WidgetDropped", "Cannot find full widget");
            }
        }
        if (populateIconsTask == null)
        {
            populateIconsTask = new LongRunningTask(PopulateIcons, LogicObject);
        }
        populateIconsTask.Start();
    }

    private string RandomString()
    {
        Guid g = Guid.NewGuid();
        return g.ToString().Replace("-", "");
    }

    private class IconElement
    {
        public string elementName;
        public float leftMargin;
        public float topMargin;
    }

    private LongRunningTask populateIconsTask;
    private LongRunningTask populateWidgetsTask;
    private PeriodicTask widgetsPositionObserverTask;
    private NodeId lastUser;

}
