using UnityEngine;
using System.Collections;

public class FileIO {

    public static void write(string filename, string content)
    { 
        System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
        file.WriteLine(content);
        file.Close();
    }


    public static string read(string filename)
    {
        return string.Join("\n",System.IO.File.ReadAllLines(filename));
    }

}
