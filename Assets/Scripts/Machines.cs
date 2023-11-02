using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using FileSystem;
using System.Linq;
using System.Text.RegularExpressions;



public class Machines : MonoBehaviour
{
    public List <Machine> machinesList = new();
    [SerializeField] TMP_InputField inputField;
    public Machine activeMachine;
    InputOutput inputOutput;

    public LocalMachine localMachine;
    string finalFileText;
    bool isFinalFileGone = false;


    public class Machine
    {
        
        public string label;
        public string IP;
        public string activeDirectory;
        public FileSystem.FileSystem fileSys;
        public bool isHacked = false;
        public Machine(string ip) 
        {

            IP = ip;
            fileSys = new(ip);

            activeDirectory = "C:\\";
            /*            foreach (Directory folder in fileSys.Directories)
                            Debug.Log(folder.path + folder.name);*/
            
        }

        

    }



    public class RemoteMachine : Machine
    {

        public RemoteMachine(string ip) : base(ip)
        {

            label = ip;
        }


    }



    public class LocalMachine : Machine
    {
        InputOutput inputOutput;
        public FileSystem.FileSystem localFileSys;
        bool isDownloading, isNonDestructive;

        List<string> commands = new()
        {
            "help",
            "cd",
            "dir",
            "ls",
            "md",
            "mv",
            "dl",
            "cp",
            "txt",
            "str",
            "stri",
            "rd",
            "cls",
            "rm",
            "ren"
        };

        Dictionary<string, string> helpOutputs = new()
        {
            { "helpGeneric help", " - displays information about all or specific command"},
            { "helpGeneric cd", " - changes directory" },
            { "helpGeneric rm", " - removes a file or directory" },
            { "helpGeneric dir", " (alias: ls) - displays a list of files and subdirectories in directory"},
            { "helpGeneric cls", " - clears the screen" },
            { "helpGeneric md", " - makes a directory" },
            { "helpGeneric txt", " - makes a .txt file" },
            { "helpGeneric str", " - replaces a string in a text file"},
            { "helpGeneric stri", " - inserts a string in a text file at specified position" },
            { "helpGeneric rd", " - prints a formated version of a text file"},
            { "helpGeneric ren", " - renames a file or directory"},
            { "helpGeneric mv", " - moves a file or directory with all its contents to a specified directory"},
            { "helpGeneric dl", " - moves a file or directory with all its contents from a remote machine to a local one"},
            { "helpGeneric cp", " - copies a file or directory with all its contents to a specified directory"},
            { "helpGeneric ip", " - moves to a machine with specified ip, or back to local machine" },




            { "helpSpecific ip", " - moves to a machine with specified ip, or back to local machine\n" + " ip [IP]\n" + "ip [192.168.1.1] \t - allows moving back to local machine" },
            { "helpSpecific dl", " - moves a file or directory with all its contents from a remote machine to a local one\n" + " dl [NAME] [NAME]\n" + " dl [NAME] [\"NAME \'...\' \"]\n" + " dl [\"NAME \'...\' \"] [\"NAME \'...\' \"]\n" + " dl [NAME] [\"PATH\"]\n" + " dl [\"NAME \'...\' \"] [\"PATH\"]"},
            { "helpSpecific mv", " - moves a file or directory with all its contents to a specified directory\n" + " mv [NAME] [NAME]\n" + " mv [NAME] [\"NAME \'...\' \"]\n" + " mv [\"NAME \'...\' \"] [\"NAME \'...\' \"]\n" + " mv [NAME] [\"PATH\"]\n" + " mv [\"NAME \'...\' \"] [\"PATH\"]"},
            { "helpSpecific cp", " - copies a file or directory with all its contents to a specified directory\n" + " cp [NAME] [NAME]\n" + " cp [NAME] [\"NAME \'...\' \"]\n" + " cp [\"NAME \'...\' \"] [\"NAME \'...\' \"]\n" + " cp [NAME] [\"PATH\"]\n" + " cp [\"NAME \'...\' \"] [\"PATH\"]"},
            { "helpSpecific help", " - displays information about all or specific command\n" + " help [COMMAND]" },
            { "helpSpecific md", " - makes a directory\n" + " md [NAME]\n" + " md [\"NAME \'...\' \"] "},
            { "helpSpecific txt", " - makes a .txt file\n" + " txt [NAME]\n" + " txt [NAME] [\"CONTENT\"]" },
            { "helpSpecific str", " - replaces a string in a text file\n" + " str [NAME] [\"STRING1\"] [\"STRING2\"]"},
            { "helpSpecific stri", " - inserts a string in a text file at specified position\n" + " stri [NAME] [\"STRING\"]\n" + " stri [NAME] [\"STRING\"] [INDEX]\n" + " Index starts at 0, any overflow is clamped to end of text\n" + " Negative indexes are counted from the end of text"},
            { "helpSpecific rd", " - prints a formated version of a text file\n" + "rd [NAME]\n"},
            { "helpSpecific ren", " - renames a file or directory\n" + " ren [\"NAME\"] [\"NEWNAME\"]\n" + " ren[\"NAME.EXT\"] [\"NEWNAME.EXT\"]"},
            { "helpSpecific cls", " - clears the screen" },
            { "helpSpecific dir", " (alias: ls) - displays a list of files and subdirectories in directory" },
            { "helpSpecific ls", " (alias: dir) - displays a list of files and subdirectories in directory" },
            { "helpSpecific cd", " - changes directory\n" + " cd [SUBDIRECTORY]\n" + " cd [\"SUBDIRECTORY \'...\' \"]\n" + " cd [..]\n" + "  ..  Specifies that you want to change to the parent directory" },
            { "helpSpecific rm", " - removes a file or directory\n" + " rm [NAME]\n" + " rm [NAME.EXT]\n" + " rm [\"NAME \'...\' \"]\n" + " rm [\"NAME \'...\'.EXT\"]\n" }
        };


        public LocalMachine(string ip): base(ip)
        {
            localFileSys = fileSys;
            this.inputOutput = GameObject.Find("InputOutput").GetComponent<InputOutput>();
            commands.Sort();
            label = Environment.UserName;
            fileSys.NewFile(Environment.UserName, "doc", "C:\\users\\", "", true);
            ScriptableFile mailApp = Resources.Load<ScriptableFile>("ScriptableObjects/Mail");
            fileSys.NewFile(mailApp.myName, mailApp.extension, mailApp.path, mailApp.content, false);
            var mails = Resources.LoadAll("ScriptableObjects/LocalMails",typeof(ScriptableFile)).Cast<ScriptableFile>();
            foreach (ScriptableFile mail in mails)
            fileSys.NewFile(mail.myName, "txt", "Mail\\", mail.content, false);
        }

        public IEnumerator ParseCommand(string command)
        {
            yield return new WaitForSeconds(0.000001f);

            /*            string[] splitCommand = command.Split(' ');
                        if (!commands.Contains(splitCommand[0].ToLower()))
                        {
                            yield return new WaitForSeconds(0.00000001f);
                            if (command.Split('.').Length > 1)
                            {
                                if (fileSys.activeDirectory.files.Exists(x => x.name == splitCommand[0].Split('.')[0] && x.extension == splitCommand[0].Split('.')[1]))
                                {
                                    File file = fileSys.activeDirectory.files.Find(x => x.name == splitCommand[0].Split('.')[0] && x.extension == splitCommand[0].Split('.')[1]);
                                    if (file.extension == "exe")
                                    {
                                        Executables(file);
                                        yield break;
                                    }
                                    inputOutput.AddText(file.content + "\n", false);
                                }
                                else
                                    inputOutput.AddText($" '{command}' is not recognized as internal or external command or program\n", false);
                                yield break;
                            }


                            inputOutput.AddText($" '{splitCommand[0]}' is not recognized as an internal or external command \n ", false);
                            yield break;
                        }

                        yield return new WaitForSeconds(0.00000001f);


                        this.GetType().GetMethod(splitCommand[0].ToLower(), BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new string[] { splitCommand.Length > 1 ? string.Join(" ",splitCommand[1..]) : "" });
            */

            /*            string content = "";
                        Regex r = new(@"(?<!\\)\u0022");
                        MatchCollection quotes = r.Matches(command);*/

            /* if (command.Split('\"').Length == 3 && command.Split("\" ").Length == 1)
             {
                 command = command.ToLower().Split('\"')[1];
                 if (command.Split('.').Length == 1)
                 {
                     inputOutput.AddText($" '{command}' is not recognized as internal or external command or program\n", false);
                     yield break;
                 }
                 if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == command.Split('.')[0] && x.extension == command.Split('.')[1]))
                 {
                     File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == command.Split('.')[0] && x.extension == command.Split('.')[1]);
                     if (file.extension == "exe")
                     {
                         Executables(file);
                         yield break;
                     }
                     inputOutput.AddText(file.content + "\n", false);
                     yield break;
                 }
             }
             else if

             if (command.Split(' ').Length > 1)
             {
                 if (!commands.Contains(command.Split(' ')[0].ToLower()))
                 {
                     inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                     yield break;
                 }

             }

             if (command.Split(' ').Length == 1)
             {
                 if (command.Split(' ')[0].Split('.').Length <= 1)
                 {
                     if (!commands.Contains(command.Split(' ')[0].ToLower()))
                     {
                         inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                         yield break;
                     }
                 }
                 if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == command.Split(' ')[0].Split('.')[0].ToLower() && x.extension == command.Split(' ')[0].Split('.')[1].ToLower()))
                 {
                     File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == command.Split(' ')[0].Split('.')[0].ToLower() && x.extension == command.Split(' ')[0].Split('.')[1].ToLower());
                     if (file.extension == "exe")
                     {
                         Executables(file);
                         yield break;
                     }
                     inputOutput.AddText(file.content + "\n", false);
                     yield break;
                 }
                 if (!commands.Contains(command.Split(' ')[0].ToLower()))
                 {
                     inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                     yield break;
                 }
             }*/

            Regex r = new(@"(?<!\\)\u0022");
            MatchCollection quotes = r.Matches(command);

            if (quotes.Count != 2 && quotes.Count != 4 && quotes.Count != 0)
            {
                if (!commands.Contains(command.Split(' ')[0]))
                {
                    inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                    yield break;
                }
                this.GetType().GetMethod(command.Split(' ')[0].ToLower(), BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new string[] { string.Join(" ", command.Split(' '))[1..] });
                yield break;

            }
            else if (quotes.Count == 0)
            {
                
                if (command.Split(' ')[0].Split('.').Length > 1)
                {
                    command = command.Split(' ')[0];
                    if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == command.Split(' ')[0].Split('.')[0].ToLower() && x.extension == command.Split(' ')[0].Split('.')[1].ToLower()))
                    {
                        File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == command.Split(' ')[0].Split('.')[0].ToLower() && x.extension == command.Split(' ')[0].Split('.')[1].ToLower());
                        if(file.isAdminProtected)
                        {
                            inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{file.name}' is protected. Use elevated mode to continue", false);
                            yield break;
                        }
                        if (file.extension == "exe")
                        {
                            Executables(file);
                            yield break;
                        }
                        inputOutput.AddText(file.content + "\n", false);
                        yield break;
                    }
                }
                if (!commands.Contains(command.Split(' ')[0]))
                {
                    inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                    yield break;
                }
            }
            else if (quotes.Count == 2)
            {
                if (command.Split('.').Length > 1 && command.Split(" \"").Length == 1)
                {
                    if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == command.Split('\"')[1].Split('.')[0].ToLower() && x.extension == command.Split('\"')[1].Split('.')[1].ToLower()))
                    {
                        File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == command.Split('\"')[1].Split('.')[0].ToLower() && x.extension == command.Split('\"')[1].Split('.')[1].ToLower());
                        if (file.extension == "exe")
                        {
                            Executables(file);
                            yield break;
                        }
                        inputOutput.AddText(file.content + "\n", false);
                        yield break;
                    }
                }
                if (!commands.Contains(command.Split(' ')[0]))
                {
                    inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                    yield break;
                }

            }
            else if (quotes.Count == 4)
            {
                if (!commands.Contains(command.Split(" ")[0]))
                {
                    inputOutput.AddText($" '{command.Split(' ')[0]}' is not recognized as internal or external command or program\n", false);
                    yield break;
                }
            }


             this.GetType().GetMethod(command.Split(' ')[0].ToLower(), BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new string[] { command.Split(' ').Length > 1 ? string.Join(" ", command.Split(' ')[1..]) : "" });

        }

        void Executables(File file)
        {
            if (file.content.Contains("mail"))
            {
                
                Instantiate(Resources.Load("Mail", typeof(GameObject)), GameObject.Find("Canvas").transform,false);
            }

            if(file.content.Contains("snake"))
            {

                if (!fileSys.Directories.Find(x => x.name == "SYS\\").files.Exists(x => x.name == "snake"))
                    fileSys.NewFile("snake", "sav", "C:\\SYS\\", "0",true);
              
                Instantiate(Resources.Load("Snek", typeof(GameObject)), GameObject.Find("Canvas").transform);
            }
        }
        void help(string command)
        {
            
            if (command == "")
            {
                foreach (string c in commands)
                {
                    if (helpOutputs.TryGetValue("helpGeneric " + c, out string help))
                        command += "\n " + c + help + "\n";
                }
                inputOutput.AddText(command, false);
            }
            else
                if (helpOutputs.TryGetValue("helpSpecific " + command, out string help))
                inputOutput.AddText($" {command}{help}", false);
            else
                inputOutput.AddText($" '{command}' is not recognized as an internal or external command \n ", false);
        }

        void cd(string path)
        {

            if(path.Split('\"').Length > 1 && path.Split('\"').Length != 3)
            {
                inputOutput.AddText($" There is no directory '{path}'", false);
                return;
            }
            else if (path.Split('\"').Length == 1)
                path = path.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            else if (path.Split('\"').Length == 3)
                path = path.Split('\"', StringSplitOptions.RemoveEmptyEntries)[0];


            int i = fileSys.ChangeDirectory(path.Split('\"',StringSplitOptions.RemoveEmptyEntries)[0]);
            if (i == 0)
            {
                activeDirectory = fileSys.activeDirectory.path == "" ? fileSys.activeDirectory.name : fileSys.activeDirectory.path + fileSys.activeDirectory.name.Trim('\\');
                inputOutput.Cd();
            }
            if (i == 1)
                inputOutput.AddText($" There is no directory '{path}'", false);
            if (i == 2)
                inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{path}' is protected. Use elevated mode to continue", false);



        }

        void dir(string attribute)
        {
            List <string> contents = new();
            
            if(attribute != "/F")
                foreach(Directory dir in fileSys.activeDirectory.contents)
                    contents.Add($"<DIR> \t\t {dir.name.Trim('\\')}");
            if(attribute != "/D")
                foreach (File file in fileSys.activeDirectory.files)
                    contents.Add($"<FILE> \t\t {file.name.Trim('\\')}.{file.extension}");
            

                
            inputOutput.AddText(string.Join('\n', contents.OrderBy(s => s.Split('\t')[2])), false);
        }

        void ls(string attribute)
        {
            dir(attribute);
        }

        void cls(string unimportant)
        {
            inputOutput.Cls();
        }

        void md(string name)
        {
            if (name.Split('\"').Length == 1)
                name = name.Split(' ')[0];
            if (name.Split('\"').Length > 1 && name.Split('\"').Length != 3)
            {
                inputOutput.AddText(" Please specify a name", false);
                return;
            }
            name = name.Split('\"', StringSplitOptions.RemoveEmptyEntries)[0];
            if (name.Split(new char[] { '\"', '\\', '/', ':', '*', '?', '>', '<' }).Length > 1)
            {
                Debug.Log(string.Join(' ', name.Split(new char[] { '\"', '\\', '/', ':', '*', '?', '>', '<' })));
                inputOutput.AddText($" A file name cannot contain special characters ('\"', '\\', '/', ':', '*', '?', '>', '<')", false);
                return;
            }
            if (fileSys.activeDirectory.contents.Exists(x => x.name.ToLower() == (name + "\\").ToLower()))
            {
                inputOutput.AddText($" Directory '{name}' already exists", false);
                return;
            }
            Debug.Log($"{name}, {activeDirectory}");

            fileSys.NewFolder(name, activeDirectory.TrimEnd('\\')+"\\");

        }

        void mv(string name)
        {
            Regex r = new(@"\u0022");
            MatchCollection quotes = r.Matches(name);
            if (quotes.Count != 2 && quotes.Count != 4 && quotes.Count != 0)
            {
                inputOutput.AddText($" '{name}' not found", false);
                return;
            }
            string path = activeDirectory;
            string extension = "";
            if (quotes.Count == 2)
            {
                if (name.Split(" \"").Length > 1)
                {
                    if (name.Split('.').Length > 1)
                        extension = name.Split(' ')[0].Split('.')[1];
                    path = name.Split('\"')[1];
                    name = name.Split(' ')[0].Split('.')[0];
                }
                else if (name.Split("\" ").Length > 1)
                {
                    if (name.Split('.').Length > 1)
                        extension = name.Split('\"')[1].Split('.')[1];
                    path = name.Split("\" ")[1];
                    name = name.Split('\"')[1].Split('.')[0];
                }
            }
            else if (quotes.Count == 4)
            {
                if(name.Split('.').Length > 1)
                    extension = name.Split('\"')[1].Split('.')[1];
                path = name.Split("\"")[3];
                name = name.Split('\"')[1].Split('.')[0];
            }
            else if (quotes.Count == 0)
            {
                if (name.Split(' ').Length == 1)
                {
                    inputOutput.AddText($" The system cannot find the directory ''", false);
                    return;
                }
                if (name.Split('.').Length > 1)
                    extension = name.Split(' ')[0].Split('.')[1];
                path = name.Split(' ')[1];
                name = name.Split(' ')[0].Split('.')[0];
            }

            switch (fileSys.Move(name.ToLower(), extension, path.ToLower(),isDownloading ? localFileSys : fileSys,isNonDestructive))
            {
                case -1:
                    {
                        inputOutput.AddText($" The system cannot find the file '{name}.{extension}'", false);
                        return;
                    }
                case 0:
                    {
                        return;
                    }
                case 1:
                    {
                        inputOutput.AddText($" The system cannot find the file '{name}'", false);
                        return;
                    }
                case 2:
                    {
                        inputOutput.AddText($" The system cannot find the directory '{path}'", false);
                        return;
                    }
                case 3:
                    {
                        inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);
                        return;
                    }
                case 4:
                    {
                        inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{path}' is protected. Use elevated mode to continue", false);
                        return;
                    }
            }
        }


        void dl(string name)
        {
            if (fileSys == localFileSys)
            {
                inputOutput.AddText($" You are not connected to a remote machine", false);
                return;
            }

            isDownloading = true;
            isNonDestructive = true;
            mv(name);
            isDownloading = false;
            isNonDestructive = false;
        }


        void cp(string name)
        {
            isNonDestructive = true;
            mv(name);
            isNonDestructive = false;
        }

        void txt(string name)
        {
            Debug.Log(activeDirectory);
            string content = "";
            Regex r = new(@"(?<!\\)\u0022");
            MatchCollection quotes = r.Matches(name);
            switch (quotes.Count)
            {
                case 0:
                    {
                        
                        if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == name.ToLower() && x.extension == "txt"))
                        {
                            inputOutput.AddText($" file '{name}.txt' already exists", false);
                            return;
                        }
                        Debug.Log($"name: {name}   dir:{activeDirectory}");
                        fileSys.NewFile(name.Split(' ')[0], "txt", activeDirectory.TrimEnd('\\')+"\\", "",false);
                        return;
                    }
                case 2:
                    {
                        if (name.Split(" \"").Length > 1)
                        {
                            content = name.Split('\"')[1];
                            name = name.Split(' ')[0];
                        }
                        else
                        {
                            name = name.Split('\"')[1];
                            content = "";
                        }    
                        break;
                    }
                case 4:
                    {
                        content = String.Join(' ', name.Substring(quotes[2].Index + 1, quotes[3].Index - quotes[2].Index - 1));
                        name = name.Split('\"')[1];
                        Debug.Log(name + ", " + content);
                        break;
                    }
                default:
                    {
                        inputOutput.AddText(" The syntax of the command is incorrect", false);
                        return;
                    }

            }

            if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == name.ToLower() && x.extension == "txt"))
            {
                inputOutput.AddText($" file '{name}.txt' already exists", false);
                return;
            }
            if (name.Split(new char[] { '\"', '\\', '/', ':', '*', '?', '>', '<' }).Length > 1)
            {
                inputOutput.AddText($" A file name cannot contain special characters ('\"', '\\', '/', ':', '*', '?', '>', '<')", false);
                return;
            }

            /*content = String.Join(' ', text.Substring(quotes[1].Index + 1, quotes[2].Index - quotes[1].Index - 1));*/
            Debug.Log($"name: {name}   dir:{activeDirectory}");
            fileSys.NewFile(name, "txt", activeDirectory.TrimEnd('\\')+'\\', content,false);
            
            /*if (name.Split(' ').Length > 1)
                fileSys.NewFile(name.Split(' ')[0].Split('.')[0], "txt", activeDirectory, string.Join(' ', name.Split(' ')[1..]));
            else
                fileSys.NewFile(name.Split('.')[0], "txt", activeDirectory, "");*/
        }

        void rm(string name)
        {
            
            if (name.Split('\"').Length == 1) 
                name = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            else if (name.Split('\"').Length == 3)
                name = name.Split('\"', StringSplitOptions.RemoveEmptyEntries)[0];

            int i = fileSys.DeleteFileOrFolder(name);
            if (i == 1)
                inputOutput.AddText($" The system cannot find the file '{name}'", false);
            else if (i == 2)
                inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);

        }

/*        void str(string name)
        {
            string[] splitName = name.Split(' ');
            switch (splitName.Length)
            {
                case 0:
                    {
                        inputOutput.AddText(" Please specify a file to edit", false);
                        break;
                    }
                case 1:
                    {
                        inputOutput.AddText(" Please specify a string to replace and a string to replace it with", false);

                        break;
                    }
                case 2:
                    {
                        inputOutput.AddText(" Please specify a string to replace the text with", false);
                        break;
                    }

                default:
                    {
                        if (fileSys.activeDirectory.files.Exists(x => x.name == splitName[0].Split('.')[0]))
                        {
                            if (string.IsNullOrEmpty(name.Split('\"')[1]))
                            {
                                inputOutput.AddText(" The string to replace cannot be empty, if you are trying to insert text into a '.txt' file, use 'stri'", false);
                                break;
                            }
                            var file = fileSys.activeDirectory.files.Find(x => x.name == splitName[0].Split('.')[0]);
                            string content = file.content;
                            var regex = new Regex(Regex.Escape(name.Split('\"')[1]));
                            file.content = regex.Replace(content, name.Split('\"')[3], 1);
                            *//*fileSys.activeDirectory.files.Find(x => x.name == splitName[0].Split('.')[0]).content = fileSys.activeDirectory.files.Find(x => x.name == splitName[0].Split('.')[0]).content.Replace(name.Split('\"')[1], name.Split('\"')[3]);*//*
                        }
                        else
                            inputOutput.AddText($" '{splitName[0]}' does not exist", false);
                        break;
                    }
            }

        }*/


        void str(string name)
        {



            Regex r = new (@"(?<!\\)\u0022");
            MatchCollection quotes = r.Matches(name);
            string replacedText = "";
            string newText = "";
            switch (quotes.Count)
            {
                case 0:
                    {
                        name = name.Split(' ')[0].ToLower();
                        break;
                    }
                case 4:
                    {
                        replacedText = name.Substring(quotes[0].Index + 1, quotes[1].Index - quotes[0].Index - 1);
                        newText = name.Substring(quotes[2].Index + 1, quotes[3].Index - quotes[2].Index - 1);
                        name = name.Split(' ')[0].Split('.')[0].ToLower();
                        /*Debug.Log($"4*\", name: {name}, {replacedText} => {newText}");*/
                        break;
                    }
                case 6:
                    {
                        replacedText = name.Substring(quotes[2].Index + 1, quotes[3].Index - quotes[2].Index - 1);
                        newText = name.Substring(quotes[4].Index + 1, quotes[5].Index - quotes[4].Index - 1);
                        name = name.Split('\"')[1].Split('.')[0].ToLower();
                        /*Debug.Log(name + ", " + replacedText + " => " + newText);*/
                        break;
                    }
                default:
                    {
                        inputOutput.AddText(" Please specify the string to replace and a string to replace it with", false);
                        return;
                    }
            }

            if (!fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == name && x.extension == "txt"))
            {
                inputOutput.AddText($" '{name.Split(' ')[0]}' does not exist or is not of '.txt' type", false);
                return;
            }
            if (name.Split(new char[] { '\"', '\\', '/', ':', '*', '?', '>', '<' }).Length > 1)
            {
                inputOutput.AddText($" A file name cannot contain special characters ('\"', '\\', '/', ':', '*', '?', '>', '<')", false);
                return;
            }

            File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == name && x.extension == "txt");

            if (file.isAdminProtected)
            {
                inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);
                return;  
            }

            if (replacedText.Length == 0)
                replacedText = file.content;

            var regex = new Regex(Regex.Escape(replacedText));

            file.content = regex.Replace(file.content, newText,1);
        }

        void stri(string name)
        {
            Regex r = new(@"(?<!\\)\u0022");
            MatchCollection quotes = r.Matches(name);
            string newText = "";
            string index = "";

            switch (quotes.Count)
            {
                case 0:
                    {
                        inputOutput.AddText(" Please, specify the string to insert", false);
                        return;
                    }
                case 2:
                    {
                        newText = name.Split('\"')[1];
                        index = name.Split('\"')[2];
                        name = name.ToLower().Split(" \"")[0].Split('.')[0].Split(' ')[0];
                        break;
                    }
                case 4:
                    {
                        newText = name.Split('\"')[3];
                        index = name.Split('\"')[4];
                        name = name.ToLower().Split('\"')[1].Split('.')[0];
                        break;
                    }
                default:
                    {
                        inputOutput.AddText(" Please, specify the file to edit and the string to insert", false);
                        return;
                    }
            }
            /*Debug.Log("index:" + index);*/

            if (fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == name && x.extension == "txt"))
            { 
                File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == name && x.extension == "txt");
                if (file.isAdminProtected == true)
                {
                    inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);
                    return;
                }
                if (Int32.TryParse(index, out int i) && i > file.content.Length)
                    index = (file.content.Length > 0 ? file.content.Length : 0).ToString();
                if (i < 0)
                {
                    index = (file.content.Length + i % (file.content.Length > 0 ? file.content.Length : 1)).ToString();
                }
                
   
                file.content = file.content.Insert(Int32.TryParse(index, out i) ? i : 0, newText);
                
            }
            else
            {
                inputOutput.AddText($" File '{name}' does not exist or is not of '.txt' type", false);
            }

            
            
        }


        

        void rd(string name)
        {
            string ext = "txt";
            if(name.Split('\"').Length > 1 && name.Split('\"').Length != 3)
            {
                inputOutput.AddText($" File '{name}' does not exist or is not of '.txt' type", false);
                return;
            }
            if (name.Split('\"').Length == 1)
                name = name.ToLower().Split(' ')[0].Split(".txt")[0];
            else
                name = name.ToLower().Split('\"')[1].Split(".txt")[0];


            if (ext == "txt" && fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == name && x.extension == "txt"))
            {
                if (fileSys.activeDirectory.files.Find(x => x.name.ToLower() == name && x.extension == "txt").isAdminProtected == true)
                {
                    inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);
                    return;
                }
                inputOutput.AddText(fileSys.activeDirectory.files.Find(x => x.name.ToLower() == name && x.extension == "txt").getRichContent(), false);
                return;
            }

            inputOutput.AddText($" File '{name}' does not exist or is not of '.txt' type", false);
        }


        void ren(string name)
        {

            if (name.Split(' ').Length <= 1 || name.Split("\"").Length != 5)
            {

                inputOutput.AddText($" Please specify a file or a directory and a new name", false);
                return;
            }
            string newName = name.Split('\"')[3];
            name = name.Split('\"')[1].ToLower();
            if(newName.Length == 0)
            {
                inputOutput.AddText(" File names cannot be empty", false);
                return;
            }

            if (newName.Split(new char[] { '\"', '\\', '/', ':', '*', '?', '>', '<' }).Length > 1)
            {
                Debug.Log(string.Join(' ', newName.Split(new char[] { '\"', '\\', '/', ':', '*', '?', '>', '<' })));
                inputOutput.AddText($" A file name cannot contain special characters ('\"', '\\', '/', ':', '*', '?', '>', '<')", false);
                return;
            }

            if (name.Split('.').Length > 1)
            {
                if(!fileSys.activeDirectory.files.Exists(x => x.name.ToLower() == name.Split('.')[0] && x.extension == name.Split('.')[1]))
                {
                    inputOutput.AddText($" File '{name}' does not exist", false);
                    return;
                }
                if(newName.Split('.',StringSplitOptions.RemoveEmptyEntries).Length<=1)
                {
                    inputOutput.AddText($" Please provide an extension for your newly named file", false);
                    return;
                }
                File file = fileSys.activeDirectory.files.Find(x => x.name.ToLower() == name.Split('.')[0] && x.extension == name.Split('.')[1]);
                if (file.isAdminProtected == true)
                {
                    inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);
                    return;
                }
                file.name = newName.Split('.')[0];
                file.extension = newName.Split('.')[1];
                return;

            }
            if(!fileSys.activeDirectory.contents.Exists(x => x.name.ToLower() == name + "\\"))
            {
                inputOutput.AddText($" Directory '{name}' does not exist", false);
                return;
            }
            Directory dir = fileSys.activeDirectory.contents.Find(x => x.name.ToLower() == name + "\\");
            if (dir.isAdminProtected == true)
            {
                inputOutput.AddText($" You do not have sufficient privilege to perform this operation. '{name}' is protected. Use elevated mode to continue", false);
                return;
            }
            dir.name = newName + "\\";

            

        }




    }




    IEnumerator ip(string ip)
    {
        if (ip == "192.168.1.1" && activeMachine.IP != ip)
        {
            activeMachine = machinesList[0];
            localMachine.fileSys = localMachine.localFileSys;
            localMachine.fileSys.activeDirectory = localMachine.fileSys.Directories[0];
            localMachine.activeDirectory = localMachine.fileSys.activeDirectory.path == "" ? localMachine.fileSys.activeDirectory.name : localMachine.fileSys.activeDirectory.path + localMachine.fileSys.activeDirectory.name.Trim('\\');
        }
        if (machinesList.Exists(x => x.IP == ip) && activeMachine.IP != ip)
        {
            if (!machinesList.Find(x => x.IP == ip).isHacked)
            {
                activeMachine = machinesList.Find(x => x.IP == ip);
                localMachine.fileSys = activeMachine.fileSys;
                localMachine.fileSys.activeDirectory = localMachine.fileSys.Directories[0];
                localMachine.activeDirectory = localMachine.fileSys.activeDirectory.path == "" ? localMachine.fileSys.activeDirectory.name : localMachine.fileSys.activeDirectory.path + localMachine.fileSys.activeDirectory.name.Trim('\\');

                GameObject hackerman = Instantiate(Resources.Load("Hackerman") as GameObject);
                hackerman.transform.SetParent(GameObject.Find("Canvas").transform,false);
                
                hackerman.transform.Find("HackermanText").GetComponent<Hackerman>().password = localMachine.fileSys.Directories.Find(x=> x.path == "C:\\" && x.name == "users\\").files.Find(x => x.extension == "doc" && x.isAdminProtected).content;

                while(hackerman != null)
                {
                    yield return null;
                }
                machinesList.Find(x => x.IP == ip).isHacked = true;
            }

            inputField.ActivateInputField();

        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        inputOutput.Cls();

    }




    private void Awake()
    {
        localMachine = new LocalMachine("192.168.1.1");
        machinesList.Add(localMachine);
        activeMachine = localMachine;
        inputOutput = GameObject.Find("InputOutput").GetComponent<InputOutput>();
        inputField.onSubmit.AddListener(delegate { if (inputField.text.ToLower().Split(' ')[0] == "ip" && inputField.text.ToLower().Split(' ').Length > 1) StartCoroutine(ip(inputField.text.ToLower().Split(' ')[1])); else StartCoroutine(localMachine.ParseCommand(inputField.text)); });

        ////////////////////////////////////////////////////////////////

        machinesList.Add(new RemoteMachine("106.90.217.75"));
        machinesList[^1].fileSys.NewFile("JohnDoe", "doc", "C:\\users\\", "okes0ftware", true);

        ScriptableFile Snake = Resources.Load<ScriptableFile>("ScriptableObjects/Snake");
        machinesList[^1].fileSys.NewFile(Snake.myName, Snake.extension, Snake.path, Snake.content, false);

        ScriptableFile mailApp = Resources.Load<ScriptableFile>("ScriptableObjects/Mail");
        machinesList[^1].fileSys.NewFile(mailApp.myName, mailApp.extension, mailApp.path, mailApp.content, false);

        var mails = Resources.LoadAll("ScriptableObjects/106", typeof(ScriptableFile)).Cast<ScriptableFile>();
        foreach (ScriptableFile mail in mails)
            machinesList[^1].fileSys.NewFile(mail.myName, "txt", "Mail\\", mail.content, false);
        
        ////////////////////////////////////////////////////////////////
        
        machinesList.Add(new RemoteMachine("42.213.76.66"));
        machinesList[^1].fileSys.NewFile("KFCow", "doc", "C:\\users\\", "admin", true);

        ScriptableFile spices = Resources.Load<ScriptableFile>("ScriptableObjects/Spices");
        machinesList[^1].fileSys.NewFile("666 secret spices", "txt", "C:\\users\\", spices.content, false);

    }

    private void Start()
    {
        ScriptableFile spices = Resources.Load<ScriptableFile>("ScriptableObjects/Spices");
        finalFileText = spices.content;
        InvokeRepeating(nameof(CheckVictory), 0, 0.5f);
    }

    void CheckVictory()
    {
        foreach (Directory dir in machinesList[^1].fileSys.Directories)
            if (dir.files.Exists(x => x.content == finalFileText))
                isFinalFileGone = false;
        if (isFinalFileGone)
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        isFinalFileGone = true;
    }

    private void Update()
    {
       
    }
}
