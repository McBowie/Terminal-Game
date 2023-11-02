using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace FileSystem
{
    public class Directory
    {
        public string name, path;
        public bool isAdminProtected = false;
        public Directory(string name, string path)
        {
            this.name = name + "\\";
            this.path = path;
        }

        public Directory(string name, string path, Directory[] contents)
        {
            this.name = name + "\\";
            this.path = path;
            this.contents.AddRange(contents);
        }
        public Directory(string name, string path, bool isAdminProtected)
        {
            this.name = name + "\\";
            this.path = path;
            this.isAdminProtected = isAdminProtected;
        }
        public Directory(string name, string path, Directory[] contents, bool isAdminProtected)
        {
            this.name = name + "\\";
            this.path = path;
            this.contents.AddRange(contents);
            this.isAdminProtected = isAdminProtected;
        }



        public List<Directory> contents = new();
        public List<File> files = new();
        



        public override int GetHashCode()
        {
            return HashCode.Combine(name, path);
        }

        public override bool Equals(object obj)
        {
            return obj is Directory directory &&
                   name == directory.name &&
                   path == directory.path;
        }
    }



    public class File
    {
        public string name, path, content, extension;
        public bool isAdminProtected = false;
        public File(string name, string extension, string path)
        {
            this.name = name;
            this.extension = extension;
            this.path = path;
            this.content = "";
        }

        public File(string name, string extension, string path, string content)
        {
            this.name = name;
            this.extension= extension;
            this.path = path;
            this.content = content;
        }

        public File(string name, string extension, string path, bool isAdminProtected)
        {
            this.name = name;
            this.extension = extension;
            this.path = path;
            this.isAdminProtected = isAdminProtected;
        }

        public File(string name, string extension, string path, string content, bool isAdminProtected)
        {
            this.name = name;
            this.extension = extension;
            this.path = path;
            this.content = content;
            this.isAdminProtected = isAdminProtected;
        }

        public string getRichContent()
        {
            string richContent;
            richContent = content.Replace("\\t", "\t");
            richContent = richContent.Replace("\\n", "\n");
            richContent = richContent.Replace("\\\"", "\"");
            return richContent;
        }

        public override bool Equals(object obj)
        {
            return obj is File file &&
                   name == file.name &&
                   path == file.path &&
                   content == file.content &&
                   extension == file.extension;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, path, content, extension);
        }
    }




    public class FileSystem
    {
        public List<Directory> Directories = new() { new Directory("C:", "", new Directory[] { new Directory("SYS", "C:\\", true), new Directory("users", "C:\\")  } ), new Directory("Mail","") } ;
        
        public Directory activeDirectory;
        public string machine;
        public FileSystem(string machine)
        {
            
                
            foreach (Directory directory in Directories[0].contents)
                Directories.Add(directory);

            this.machine = machine;
            activeDirectory = Directories[0];
        }

        public Directory NewFolder(string name, string path)
        {

            var newDir = new Directory(name, path);
            Directories.Add(newDir);
            Directories.Find(x => path == x.path + x.name).contents.Add( newDir );

            return newDir;
        }

        public int DeleteFileOrFolder(string name)
        {
            Debug.Log(name);
            Debug.Log(activeDirectory.files.Find(x => x.name.ToLower() == name.ToLower().Split('.')[0]+"\\"));
            if (activeDirectory.contents.Exists(x => x.name.ToLower().Trim('\\') == name.ToLower()))
            {
                name += "\\";
                Directory dir = activeDirectory.contents.Find(x => x.name.ToLower() == name.ToLower());

                if(dir.isAdminProtected)
                    return 2;

                Directories.Remove(dir);
                activeDirectory.contents.Remove(dir);

                return 0;
            }
            else if (!name.Contains('.'))
                return 1;
            else if (activeDirectory.files.Exists(x => x.name.ToLower() == name.Split('.')[0].ToLower() && x.extension == name.Split('.')[1].ToLower()))
            {
                File file = activeDirectory.files.Find(x => x.name.ToLower() == name.Split('.')[0].ToLower() && x.extension == name.Split('.')[1].ToLower());

                if (file.isAdminProtected)
                    return 2;

                activeDirectory.files.Remove(file);

                return 0;
            }
            return 1;
            

        }

        public int ChangeDirectory(string name)
        {
            name = name.ToLower() + "\\";
            if (name == "..\\")
            {
                if (activeDirectory.path != "")
                {
                    activeDirectory = Directories.Find(x => activeDirectory.path.Split('\\')[^2] + "\\" == x.name);
                    return 0;
                }
                return 1;
            }
            if (activeDirectory.contents.Exists(x => x.name.ToLower() == name))
            {
                if (activeDirectory.contents.Find(x => x.name.ToLower() == name).isAdminProtected)
                    return 2;
                activeDirectory = activeDirectory.contents.Find(x => x.name.ToLower() == name);
                return 0;
            }
            return 1;
            


            //Debug.Log("Active dir: " + activeDirectory.path + activeDirectory.name);
        }

        public File NewFile(string name, string extension, string path, string content, bool isProtected)
        {
            File file;
            if (content == "")
                file = new File(name, extension, path, isProtected);
            else
                file = new File(name, extension, path, content, isProtected);

            Directories.Find(x => file.path == x.path + x.name).files.Add(file);

            return file;
        }




        public int Move(string name, string extension, string path, FileSystem sys,bool isNonDestructive)
        {
            name += "\\";
            path += "\\";
          /*Debug.Log(activeDirectory.files.Exists(x => x.name.ToLower() == name.ToLower()));
            Debug.Log($"name: {name} .{extension}");
            Debug.Log(activeDirectory.path.ToLower() + activeDirectory.name.ToLower() + " => " + path);*/

            Directory newDir = sys.Directories.Find(x => x.path.ToLower() + x.name.ToLower() == path || (x.path.ToLower() + x.name.ToLower() == activeDirectory.path.ToLower() + activeDirectory.name.ToLower() + path)) ?? activeDirectory.contents.Find(x => x.name.ToLower() == path);
            if (newDir == null)
                return 2;
            if (newDir.isAdminProtected)
                return 4;
            if (extension == "" && activeDirectory.contents.Exists(x => x.name.ToLower() == name))
            {
                Directory dir;
                
                dir = activeDirectory.contents.Find(x => x.name.ToLower() == name);
               
                if (dir.isAdminProtected)
                    return 3;
                if (dir == null)
                    return 1;

                if (isNonDestructive)
                {
                    name = dir.name;

                        
                    foreach(Directory d in newDir.contents)
                    {
                        Debug.Log($"newdir: {d.path + d.name}");
                    }
                    Regex r = new Regex(@"(\([0-9]*\)\\$)");
                    if (!r.IsMatch(name))
                    {
                        name = name.Insert(name.Length-1, "(1)");
                    }

                    while (newDir.contents.Exists(x => x.name.ToLower() == name.ToLower()))
                    {
                        
                        int num = int.Parse(r.Match(name).Value.Trim(new char[] { '(', ')', '\\' }));
                        name = name.Remove(r.Match(name).Index);
                        name = name.Insert(name.Length,$"({(num+1)})\\");
                        /*name = name.Trim('\\') + "(Copy)\\";*/
                    }

                    Directory copy = sys.NewFolder(name.Trim('\\'), newDir.path + newDir.name);
                    copy.contents.AddRange(dir.contents);
                    return 0;
                }

                dir.path = newDir.path + newDir.name;
                activeDirectory.contents.Remove(dir);
                newDir.contents.Add(dir);
                foreach (Directory direc in newDir.contents)
                {
                    Debug.Log(direc.name);
                }
                return 0;
            }


            if (activeDirectory.files.Exists(x => x.name.ToLower() == name.Trim('\\') && x.extension.ToLower() == extension))
            {
                File file;

                file = activeDirectory.files.Find(x => x.name.ToLower() == name.Trim('\\') && x.extension == extension);

                if (file.isAdminProtected)
                    return 3;
                if (file == null)
                    return 1;

                file.path = newDir.path + newDir.name;
                if (isNonDestructive)
                {

                    /*copy.name = copy.name.Trim('\\') + */


                        name = file.name;

                    


                    while (newDir.files.Exists(x => x.name.ToLower() == name.ToLower() && x.extension == extension))
                    {
                        Regex r = new Regex(@"(\([0-9]*\)$)");
                        if (!r.IsMatch(name))
                        {
                            name = name.Insert(name.Length, "(0)");
                        }
                        /*name = name.Trim('\\') + "(Copy)";*/

                        
                        int num = int.Parse(r.Match(name).Value.Trim(new char[] { '(', ')', '\\' }));
                        name = name.Remove(r.Match(name).Index);
                        name = name.Insert(name.Length, $"({(num + 1)})");
                    }

                    File copy = sys.NewFile(name.TrimEnd('\\'), extension, newDir.path + newDir.name, file.content,false);
                    return 0;
                }
                activeDirectory.files.Remove(file);
                newDir.files.Add(file);
                return 0;
            }
            return -1;
        }

    }
}
