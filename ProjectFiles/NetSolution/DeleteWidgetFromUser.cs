#region Using directives
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Linq;
#endregion

public class DeleteWidgetFromUser : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void DeleteWidget()
    {
        var widgetsToDelete = Session.User.Children.OfType<UserWidget>().Where(item => item.WidgetBrowseName == Owner.BrowseName);
        foreach (var item in widgetsToDelete)
        {
            item.Delete();
        }
        Owner.Delete();
    }
}
