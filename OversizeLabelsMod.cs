using PiTung;
using PiTung.Components;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
public class OversizeLabels : Mod
{
    public override string Name => "Oversize Labels For PiTUNG 2.4.X";
    public override string PackageName => "me.jimmy.StretchedLabels.PiTUNG2.4.X";
    public override string Author => "Iamsodarncool, Modified by Ryan";
    public override Version ModVersion => new Version("1.1");

    public override void BeforePatch()
    {
        ComponentRegistry.CreateNew<CustomLabel>("WidePanelLabel", "Wide Panel Label", CreatePanelLabelOfSize(3, 1));
        ComponentRegistry.CreateNew<CustomLabel>("TallPanelLabel", "Tall Panel Label", CreatePanelLabelOfSize(1, 3));
        ComponentRegistry.CreateNew<CustomLabel>("BigPanelLabel", "Big Panel Label", CreatePanelLabelOfSize(2, 2));

        // This game's sequal has been in devolpment for over 2 years. I hope the world is ready
        ComponentRegistry.CreateNew<CustomLabel>("CollosallyWidePanelLabel", "Collosally Wide Panel Label", CreatePanelLabelOfSize(51, 1));
        ComponentRegistry.CreateNew<CustomLabel>("CollosallyTallPanelLabel", "Collosally Tall Panel Label", CreatePanelLabelOfSize(1, 51));
        // On second thought, the world is still not ready to see this one
        // ComponentRegistry.CreateNew<CustomLabel>("TitanicPanelLabel", "Titanic Panel Label", PanelLabelOfSize(400, 700));

        // add labels from file
        if (File.Exists($"{Directory.GetCurrentDirectory()}/sizes_l.txt"))
        {
            string[] sizes = File.ReadAllLines($"{Directory.GetCurrentDirectory()}/sizes_l.txt");
            foreach (string size in sizes)
            {
                try
                {
                    int[] xysize = Array.ConvertAll(size.Split(' '), s => int.Parse(s)); // split the string into a string array at every space and convert that into an int array
                    ComponentRegistry.CreateNew($"{xysize[0]} x {xysize[1]} PanelLabel", $"{xysize[0]} x {xysize[1]} PanelLabel", CreatePanelLabelOfSize(xysize[0], xysize[1]));
                }
                catch (Exception ex) // Catches all exceptions
                {
                    if (ex is ArgumentException)
                    {
                        IGConsole.Log($"Error! Cant load label of size {size} twice");
                    }
                    else
                    {
                        IGConsole.Log($"Error! Cant load label of size {size}, correct format is \"{{X size of label}} {{Y size of label}}\" ");
                    }
                }
            }
        }
        Shell.RegisterCommand<Add_Label>();
        Shell.RegisterCommand<Remove_Label>();
    }

    public static CustomBuilder CreatePanelLabelOfSize(int x, int z)
    {
        return PrefabBuilder
            .Custom(() =>
            {
                var obj = new GameObject();
                var label = UnityEngine.Object.Instantiate(References.Prefabs.PanelLabel, obj.transform);
                label.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(x, z);

                // remove the existing white block of the label, since we're replacing it with a bigger one
                MegaMeshManager.RemoveComponentsImmediatelyIn(label);
                UnityEngine.Object.Destroy(label.GetComponent<MegaMeshComponent>());
                UnityEngine.Object.Destroy(label.GetComponent<Renderer>());
                UnityEngine.Object.Destroy(label.GetComponent<MeshFilter>());

                // create the new geometry
                var geometry = UnityEngine.Object.Instantiate(References.Prefabs.WhiteCube, obj.transform);
                geometry.transform.localScale = new Vector3(x * 0.3f, 0.1f, z * 0.3f);

                // make sure the collider of the label is big enough, so you can click on all parts of it
                label.GetComponent<BoxCollider>().size = new Vector3(x, 1, z);

                // ...and get rid of the collider of the geometry, so it doesn't interfere with clicking on the label.
                // Destorying it causes bugs for some reason so I just do this ¯\_(ツ)_/¯
                geometry.GetComponent<BoxCollider>().size = Vector3.zero;

                // if it is an even number high, we have to shift everything in the component so that it still lines up with the grid
                if (z % 2 == 0)
                {
                    for (int i = 0; i < obj.transform.childCount; i++)
                    {
                        obj.transform.GetChild(i).transform.localPosition += new Vector3(0, 0, 0.15f);
                    }
                }
                // ditto if it is an even number wide
                if (x % 2 == 0)
                {
                    for (int i = 0; i < obj.transform.childCount; i++)
                    {
                        obj.transform.GetChild(i).transform.localPosition += new Vector3(0.15f, 0, 0);
                    }
                }

                return obj;
            });
    }
}

public class Add_Label : Command
{
    public override string Name => "addLabel";
    public override string Usage => $"{Name} x_size y_size";
    public override string Description => "Adds a Label of specified size to the component list";

    public override bool Execute(IEnumerable<string> args)
    {
        if (args.Count() < 2)
        {
            IGConsole.Log("Not enough arguments!");
            return false;
        }
        string size = string.Join(" ", args.ToArray()); //turns the array into a string
        try
        {
            int[] xysize = Array.ConvertAll(size.Split(' '), s => int.Parse(s)); // split the string into a string array at every space and convert that into an int array
            ComponentRegistry.CreateNew($"{xysize[0]} x {xysize[1]} PanelLabel", $"{xysize[0]} x {xysize[1]} PanelLabel", OversizeLabels.CreatePanelLabelOfSize(xysize[0], xysize[1]));
        }
        catch (Exception ex) // Catches all exceptions
        {
            if (ex is ArgumentException)
            {
                IGConsole.Log($"Error! Cant make Label of size {size}, It already exists!");
            }
            else
            {
                IGConsole.Log($"Error! Cant make Label of size {size}, correct format is \"{{X size of Label}} {{Y size of Label}}\" ");
            }
            return false;
        }
        string[] file = { };
        if (File.Exists($"{Directory.GetCurrentDirectory()}/sizes_l.txt"))
        {
            file = File.ReadAllLines($"{Directory.GetCurrentDirectory()}/sizes_l.txt");
        }
        List<string> list = new List<string>();
        list.AddRange(file);
        list.AddRange(new string[] { size });
        file = list.ToArray();
        File.WriteAllLines($"{Directory.GetCurrentDirectory()}/sizes_l.txt", file);
        IGConsole.Log($"Added Label of size {size} to component list");
        return true;
    }
}

public class Remove_Label : Command
{
    public override string Name => "removeLabel";
    public override string Usage => $"{Name} x_size y_size";
    public override string Description => "removes a Label of specified size to the component list";

    public override bool Execute(IEnumerable<string> args)
    {
        if (args.Count() < 2)
        {
            IGConsole.Log("Not enough arguments!");
            return false;
        }
        string size = string.Join(" ", args.ToArray()); //turns the array into a string
        try
        {
            int[] xysize = Array.ConvertAll(size.Split(' '), s => int.Parse(s)); // split the string into a string array at every space and convert that into an int array
        }
        catch (Exception ex) // Catches all exceptions
        {
            IGConsole.Log($"Error! Cant remove Label of size {size}, correct format is \"{{X size of Label}} {{Y size of Label}}\" ");
            return false;
        }
        string[] file = { };
        if (File.Exists($"{Directory.GetCurrentDirectory()}/sizes_l.txt"))
        {
            file = File.ReadAllLines($"{Directory.GetCurrentDirectory()}/sizes_l.txt");
            if (file.Contains(size))
            {
                List<string> fileList = file.ToList();
                fileList.Remove(size);
                file = fileList.ToArray();
            }
            else
            {
                IGConsole.Log($"Error! Cant remove Label of size {size}, It doesn't exist!");
                return false;
            }
            File.WriteAllLines($"{Directory.GetCurrentDirectory()}/sizes_l.txt", file);
        }
        else
        {
            IGConsole.Log($"Error! Cant remove Label of size {size}, There are no custom Labels to delete!");
            return false;
        }
        IGConsole.Log($"Reomved Label of size {size} from component list. NOTE: this change will only take effect when you restart your game!!!");
        return true;
    }
}
