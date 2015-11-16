﻿using System.Collections.Generic;

namespace Workshop.Recipes
{
    using UnityEngine;

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class WorkshopRecipeDatabase : MonoBehaviour
    {
        public static PartRecipe DefaultPartRecipe;

        public static Dictionary<string, PartRecipe> PartRecipes;

        public static Dictionary<string, Recipe> ResourceRecipes;

        public static bool HasResourceRecipe(string name)
        {
            return ResourceRecipes.ContainsKey(name);
        }

        public static Blueprint ProcessPart(AvailablePart part)
        {
            var resources = new Dictionary<string, WorkshopResource>();
            if (PartRecipes.ContainsKey(part.name))
            {
                var recipe = PartRecipes[part.name];
                foreach (var workshopResource in recipe.Prepare(part.partPrefab.mass, part.cost))
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
                foreach (var workshopResource in DefaultPartRecipe.Prepare(part.partPrefab.mass, part.cost))
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

            foreach (PartResource partResource in part.partPrefab.Resources)
            {
                if (ResourceRecipes.ContainsKey(partResource.resourceName))
                {
                    var definition = PartResourceLibrary.Instance.GetDefinition(partResource.resourceName);
                    var recipe = ResourceRecipes[partResource.resourceName];
                    foreach (var workshopResource in recipe.Prepare(partResource.maxAmount * definition.density))
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
            PartRecipes = new Dictionary<string, PartRecipe>();
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
