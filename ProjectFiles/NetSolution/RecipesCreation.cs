#region Using directives
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.Store;
using System;
using System.Threading;
using UAManagedCore;
#endregion

public class RecipesCreation : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        recipesCreator = new LongRunningTask(CreateRecipes, LogicObject);
        recipesCreator.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        recipesCreator?.Dispose();
    }

    private void CreateRecipes()
    {
        // Check if we have any recipe in the store
        var myStore = Project.Current.Get<Store>("DataStores/EmbeddedDatabase");
        while (myStore.Status != StoreStatus.Online)
        {
            Thread.Sleep(500);
        }
        Object[,] ResultSet;
        String[] Header;
        myStore.Query("SELECT * FROM RecipeSchema", out Header, out ResultSet);
        if (ResultSet.Length > 0)
            return;
        // Add recipes to the store
        var myTable = myStore.Tables.Get<Table>("RecipeSchema");
        string[] columns = { "Name", "/BoilerLevelSetpoint", "/BoilerTempSetpoint" };
        var values = new object[3, 3];
        values[0, 0] = "Recipe 1";
        values[0, 1] = 101;
        values[0, 2] = 91;
        values[1, 0] = "Recipe 2";
        values[1, 1] = 100;
        values[1, 2] = 90;
        values[2, 0] = "Recipe 3";
        values[2, 1] = 99;
        values[2, 2] = 89;
        myTable.Insert(columns, values);
        Log.Debug("RecipesCreation.CreateRecipes", "Recipes creation completed");
        recipesCreator?.Dispose();
    }

    private LongRunningTask recipesCreator;
}
