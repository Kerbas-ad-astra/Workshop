﻿using System.Collections.Generic;

namespace Workshop.Recipes
{
    using UnityEngine;

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class WorkshopRecipeDatabase : MonoBehaviour
    {
        public static Recipe DefaultRecipe;

        public static Dictionary<string, Recipe> PartRecipes;

        public static Dictionary<string, Recipe> ResourceRecipes;

        public static Blueprint ProcessPart(Part part)
        {
            print("[OSE] Processing " + part.name);
            var resources = new Dictionary<string, WorkshopResource>();
            if (PartRecipes.ContainsKey(part.name))
            {
                print("[OSE] Recipe found");
                var recipe = PartRecipes[part.name];
                foreach (var workshopResource in recipe.Prepare(part.mass))
                {
                    if (resources.ContainsKey(workshopResource.Name))
                    {
                        resources[workshopResource.Name].Merge(workshopResource);
                    }
                    else
                    {
                        resources[workshopResource.Name] = workshopResource;
                    }
                }
            }
            else
            {
                print("[OSE] No recipe found, using default!");
                foreach (var workshopResource in DefaultRecipe.Prepare(part.mass))
                {
                    if (resources.ContainsKey(workshopResource.Name))
                    {
                        resources[workshopResource.Name].Merge(workshopResource);
                    }
                    else
                    {
                        resources[workshopResource.Name] = workshopResource;
                    }
                }
            }

            foreach (PartResource partResource in part.Resources)
            {
                if (ResourceRecipes.ContainsKey(partResource.resourceName))
                {
                    print("[OSE] Resource recipe found");
                    var recipe = ResourceRecipes[partResource.resourceName];
                    foreach (var workshopResource in recipe.Prepare(part.mass))
                    {
                        if (resources.ContainsKey(workshopResource.Name))
                        {
                            resources[workshopResource.Name].Merge(workshopResource);
                        }
                        else
                        {
                            resources[workshopResource.Name] = workshopResource;
                        }
                    }
                }
            }

            var blueprint = new Blueprint();
            blueprint.AddRange(resources.Values);
            return blueprint;
        }

        void Awake()
        {
            PartRecipes = new Dictionary<string, Recipe>();
            ResourceRecipes = new Dictionary<string, Recipe>();

            var loaders = LoadingScreen.Instance.loaders;
            if (loaders != null)
            {
                for (var i = 0; i < loaders.Count; i++)
                {
                    var loadingSystem = loaders[i];
                    if (loadingSystem is WorkshopRecipeLoader)
                    {
                        print("[OSE] found WorkshopRecipeLoader: " + i);
                        (loadingSystem as WorkshopRecipeLoader).Done = false;
                        break;
                    }
                    if (loadingSystem is PartLoader)
                    {
                        print("[OSE] found PartLoader: " + i);
                        var go = new GameObject();
                        var recipeLoader = go.AddComponent<WorkshopRecipeLoader>();
                        loaders.Insert(i, recipeLoader);
                        break;
                    }
                }
            }
        }
    }
}
