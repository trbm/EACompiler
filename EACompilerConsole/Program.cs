using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using EA;


namespace EACompilerII
{
    public class Program
    {
        const string FILESPEC = "*.eap";

        private static string sVersion = "";
        private static string sOptions = "";
        private static string sRootSearchDirectory = "";
        private static string ENVIRONMENT_VARIBLE_NAME_ROOT_DIR = "TLV_DIRECTORY";
        private static string TLV_SET_DIRECTORY_NAME = "TLV_SET_TEMPORARY_DIRECTORY";
        private static string SEVEN_ZIP_X86_EXE = @"C:\Program Files (x86)\7-Zip\7zG.exe";
        private static string SEVEN_ZIP_EXE = @"C:\Program Files\7-Zip\7zG.exe";
        private static string SEVEN_ZIP_VERIFIED_EXE = "";
        private static string ZIP_COMMAND_FILENAME = "zip.cmd";
        private static string TLV_SET_ZIPPED_FILENAME = "TLVfileSet.zip";

        private static List<string> DirectoryReferenceList = new List<string>();
        private static List<string> FileReferenceList = new List<string>();

        public static bool TLVonly { get; set; }
        public static bool ModelsOnly { get; set; }
        public static bool ShowFilenames { get; set; }
        public static bool DumpFileContents { get; set; }
        public static bool BuildDocumentation { get; set; }

        static void Main(string[] args)
        {
            try
            {
                sVersion = getAssemblyVersion();
                IntermediateRepresentationXML.bSuppressCompletionSound = true;
                sjmErrorHandler.SuppressDialogBox = true;                   // never show a dialog box on error 

                Console.WriteLine("    ____________________________________________________");
                Console.WriteLine("    EACompiler Console  (v" + getAssemblyVersion() + ")");
                Console.WriteLine("");
                if (scanInputArguments(args))
                {
                    Console.WriteLine("    Options: ");
                    Console.WriteLine(sOptions);
                    Console.WriteLine("");

                    compileModels();
                    parseForTLVs();
                    createTLVset();
                }
            }
            catch (Exception e)
            {
                sjmErrorHandler oErrorHandler = new sjmErrorHandler(e);
            }
        }

        private static bool verify7Zip()
        {
            bool bFoundIt = true;
            if (System.IO.File.Exists(SEVEN_ZIP_EXE))
            {
                SEVEN_ZIP_VERIFIED_EXE = SEVEN_ZIP_EXE;
            }
            else
            {
                if (System.IO.File.Exists(SEVEN_ZIP_X86_EXE))
                {
                    SEVEN_ZIP_VERIFIED_EXE = SEVEN_ZIP_X86_EXE;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("");
                    Console.WriteLine("This computer doesn't appear to have '7-Zip' installed (neither " + SEVEN_ZIP_EXE + " or " + SEVEN_ZIP_X86_EXE + ").");
                    Console.WriteLine("Without 7-Zip you will not be able to build a complete set of auxillary TLV files.");
                    Console.WriteLine("Please download it at: http://www.7-zip.org/download.html");
                    Console.WriteLine("");
                    Console.ResetColor();
                    Console.WriteLine("Press ENTER to continue...");
                    Console.ReadLine();
                    bFoundIt = false;
                }
            }
            return bFoundIt;
        }

        private static void createTLVset()
        {
            if (verify7Zip())
            {
                try
                {
                    if (Directory.Exists(TLV_SET_DIRECTORY_NAME))
                    {
                        Directory.Delete(path: TLV_SET_DIRECTORY_NAME, recursive: true);
                    }
                    Directory.CreateDirectory(TLV_SET_DIRECTORY_NAME);           // create a temporary directory

                    string sMaximumCommonPath = findMaximumCommonPath();
                    recurseForTLVfiles(sMaximumCommonPath);

                    foreach (string sFullFilename in FileReferenceList)
                    {
                        string sNewFilename = TLV_SET_DIRECTORY_NAME + "\\" + Path.GetFileName(sFullFilename);
                        if (!System.IO.File.Exists(sNewFilename))
                        {
                            System.IO.File.Copy(sFullFilename, sNewFilename);
                        }
                    }
                    zipTLVset(sMaximumCommonPath);
                }

                catch (Exception e)
                {
                    sjmErrorHandler oErrorHandler = new sjmErrorHandler(e);
                }
            }
        }

        private static void zipTLVset(string sCommonDirectoryRoot)
        {
            try
            {
                string sTemporaryCommandFilename = TLV_SET_DIRECTORY_NAME + "\\" + ZIP_COMMAND_FILENAME;
                if (System.IO.File.Exists(TLV_SET_ZIPPED_FILENAME))
                {
                    System.IO.File.Delete(TLV_SET_ZIPPED_FILENAME);
                }

                OutputFile oTemporaryCommandFile = new OutputFile(sTemporaryCommandFilename);
                oTemporaryCommandFile.AddLine("\"" + SEVEN_ZIP_VERIFIED_EXE + "\" a   \"" + sCommonDirectoryRoot + "\\" + TLV_SET_ZIPPED_FILENAME + "\"        *.*");
                oTemporaryCommandFile.Close();

                Process p = new Process();
                p.StartInfo.WorkingDirectory = TLV_SET_DIRECTORY_NAME;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = ZIP_COMMAND_FILENAME;
                p.Start();
                p.WaitForExit();

                Directory.Delete(path: TLV_SET_DIRECTORY_NAME, recursive: true);
            }
            catch (Exception e)
            {
                sjmErrorHandler oErrorHandler = new sjmErrorHandler(e);
            }
        }

        private static void recurseForTLVfiles(string sParentDirectoryName)
        {
            try
            {
                DirectoryInfo oDirectoryInfo = new DirectoryInfo(sParentDirectoryName);
                if ((oDirectoryInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    foreach (string sDirectoryName in Directory.GetDirectories(sParentDirectoryName))
                    {
                        recurseForTLVfiles(sDirectoryName);
                        foreach (string sFullFilename in Directory.GetFiles(sDirectoryName, "*" + SourceTLVParser.FILE_EXTENSION_ADDITION_XML))
                        {
                            addUniqueFileReference(sFullFilename);
                        }

                        string sDerivedDataTypesFilename = sDirectoryName + "\\DerivationDataTypes.xml";
                        if (System.IO.File.Exists(sDerivedDataTypesFilename))
                        {
                            addUniqueFileReference(sDerivedDataTypesFilename);
                        }

                        string sDerivationSourceFilename = sDirectoryName + "\\DerivationSource.xml";
                        if (System.IO.File.Exists(sDerivationSourceFilename))
                        {
                            addUniqueFileReference(sDerivationSourceFilename);
                        }

                        string sRemoteControlXSDfilename = sDirectoryName + "\\RemoteControlExample.xsd";
                        if (System.IO.File.Exists(sRemoteControlXSDfilename))
                        {
                            addUniqueFileReference(sRemoteControlXSDfilename);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                sjmErrorHandler oErrorHandler = new sjmErrorHandler(e);
            }
        }

        private static string findMaximumCommonPath()
        {
            string sMaximumCommonPath = "";
            try
            {
                if (DirectoryReferenceList.Count > 0)
                {
                    string[] oReferenceTokens = Regex.Split(DirectoryReferenceList[0], "[\\\\/]");      // split the full path into individual folder names
                    foreach (string sReferenceDirectoryPathFolderName in oReferenceTokens)
                    {
                        bool bAllHaveFolderInCommon = true;
                        foreach (string sDirectoryName in DirectoryReferenceList)
                        {
                            if (!sDirectoryName.Contains(sReferenceDirectoryPathFolderName))        // if this directory does not have the present folder in common
                            {
                                bAllHaveFolderInCommon = false;
                                break;
                            }
                        }
                        if (bAllHaveFolderInCommon)
                        {
                            sMaximumCommonPath += sReferenceDirectoryPathFolderName + "\\";
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                sjmErrorHandler oErrorHandler = new sjmErrorHandler(e);
            }
            return sMaximumCommonPath;
        }

        private static void compileModels()
        {
            if (!TLVonly)
            {
                BuildModelsRecursively(BuildDocumentation);
                Console.WriteLine("    ");
            }
        }

        private static void addUniqueDirectoryReference(string sDirectoryName)
        {
            DirectoryInfo oDirectoryInfo = new DirectoryInfo(sDirectoryName);
            string sFullyQualifiedDirectoryName = oDirectoryInfo.FullName;
            if (!DirectoryReferenceList.Contains(sFullyQualifiedDirectoryName))
            {
                DirectoryReferenceList.Add(sFullyQualifiedDirectoryName);
            }
        }

        private static void addUniqueFileReference(string sFileName)
        {
            if (!FileReferenceList.Contains(sFileName))
            {
                FileReferenceList.Add(sFileName);
            }
        }

        private static void parseForTLVs()
        {
            try
            {
                if (!ModelsOnly)
                {
                    if (sRootSearchDirectory.Length == 0)
                    {
                        sRootSearchDirectory = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIBLE_NAME_ROOT_DIR);
                        if (sRootSearchDirectory == null)
                        {
                            sRootSearchDirectory = "intentionally bad directory name";
                        }

                        if (!Directory.Exists(sRootSearchDirectory))
                        {
                            sRootSearchDirectory = ".";
                        }
                    }
                    addUniqueDirectoryReference(sRootSearchDirectory);
                    SourceTLVParser oSourceTLVParser = new SourceTLVParser(sRootSearchDirectory, ShowFilenames, DumpFileContents);
                    Console.WriteLine("    ");
                }
            }
            catch (Exception e)
            {
                sjmErrorHandler oErrorHandler = new sjmErrorHandler(e);
            }
        }

        static string getAssemblyVersion()
        {
            System.Reflection.Assembly oAssembly = System.Reflection.Assembly.GetCallingAssembly();
            string[] oTokens = Regex.Split(oAssembly.FullName, ",");
            string sRawVersionString = oTokens[1].Substring(oTokens[1].LastIndexOf("=") + 1);
            return sRawVersionString;
        }

        private static bool scanInputArguments(string[] args)
        {
            bool bRun = true;
            foreach (string sArgument in args)
            {
                switch (sArgument.ToUpper())
                {
                    case "DOCUMENTATION":
                    case "D":
                    case "/D":
                    case "-D":
                        sOptions += "       Documentation = generate documentation from the models\n";
                        BuildDocumentation = true;
                        break;

                    case "DUMP":
                        DumpFileContents = true;
                        sOptions += "       Dump = show the entire contents of any matching .c files\n";
                        break;

                    case "FILES":
                        sOptions += "       Files = show fully qualified path name of any matching .c files\n";
                        ShowFilenames = true;
                        break;

                    case "VERSION":
                    case "V":
                    case "/V":
                    case "-V":
                        showVersion();
                        bRun = false;
                        break;

                    case "/?":
                    case "?":
                    case "-?":
                    case "/HELP":
                    case "HELP":
                        showHelp();
                        bRun = false;
                        break;

                    case "TLV":
                    case "/TLV":
                    case "-TLV":
                        sOptions += "       TLV = scan for foreground TLVs only\n";
                        ModelsOnly = false;
                        TLVonly = true;
                        break;

                    case "NOTLV":
                    case "/NOTLV":
                    case "-NOTLV":
                        sOptions += "       NoTLV = suppress scanning for foreground TLVs\n";
                        ModelsOnly = true;
                        TLVonly = false;
                        break;

                    default:
                        if (Directory.Exists(sArgument))
                        {
                            sRootSearchDirectory = sArgument;
                            sOptions += "       " + sArgument + " = search directory\n";
                        }
                        else
                        {
                            Console.WriteLine("Unknown switch on the command line: " + sArgument);
                        }
                        break;
                }
            }
            return bRun;
        }

        private static void showVersion()
        {
            Console.WriteLine("EA Compiler version " + sVersion);
        }

        private static void showHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("_____________________________________________________________________________");
            Console.WriteLine("");
            showVersion();
            Console.WriteLine("");
            Console.WriteLine("    Help \t\tShow this help message");
            Console.WriteLine("");
            Console.WriteLine("    <valid directory> \tStart TLV search here (or set " + ENVIRONMENT_VARIBLE_NAME_ROOT_DIR + ")");
            Console.WriteLine("");
            Console.WriteLine("    Documentation \tGenerate documentation from the models");
            Console.WriteLine("    TLV \t\tScan for foreground TLVs only (skip model compiling)");
            Console.WriteLine("    NoTLV \t\tSuppress scanning for foreground TLVs");
            Console.WriteLine("    Files \t\tShow fully qualified filenames for matching .c (parsed) files");
            Console.WriteLine("    Dump \t\tDump the entire text of any matching .c (parsed) files");
            Console.WriteLine("_____________________________________________________________________________");
            Console.WriteLine("");

        }

        public static void BuildModelsRecursively(bool BuildDocumentation)
        {
            Console.WriteLine("  ");
            Console.WriteLine("    Creating a new repository instance");
            Repository oRepository = new Repository();
            Console.WriteLine("    Creating a new addin instance");
            AddIn oAddIn = new AddIn();

            buildModels(oRepository, oAddIn, ".", BuildDocumentation);
        }

        private static void buildModels(Repository oRepository, AddIn oAddIn, string sParentDirectoryName, bool BuildDocumentation)
        {
            foreach (string sDirectoryName in Directory.GetDirectories(sParentDirectoryName))
            {
                addUniqueDirectoryReference(sDirectoryName);
                buildModels(oRepository, oAddIn, sDirectoryName, BuildDocumentation);      // build all models at this level
                foreach (string sFullFilename in Directory.GetFiles(sDirectoryName, FILESPEC))
                {
                    buildOneModel(oRepository, oAddIn, sFullFilename, BuildDocumentation);
                }
            }
        }

        private static void buildOneModel(Repository oRepository, AddIn oAddIn, string sFullFilename, bool BuildDocumentation)
        {
            FileInfo oFileInfo = new FileInfo(sFullFilename);
            bool bIsReadonly = oFileInfo.IsReadOnly;

            oFileInfo.IsReadOnly = false;           // allow file write, otherwise EA won't open it

            Console.WriteLine("");
            Console.WriteLine("        Opening the model file: " + Path.GetFileName(oFileInfo.FullName));
            oRepository.OpenFile(oFileInfo.FullName);

            Console.WriteLine("        Connecting to the model repository");
            oAddIn.EA_Connect(oRepository);

            if (BuildDocumentation)
            {
                Console.WriteLine("        Compiling the model (and generating documentation)");
            }
            else
            {
                Console.WriteLine("        Compiling the model       (" + sFullFilename + ")");
            }
            oAddIn.CompileOneModel(oRepository, BuildDocumentation, "<ignored location>", "<ignored menuName>", AddIn.VersionString);

            oFileInfo.IsReadOnly = bIsReadonly;           // restore the readonly status
            oRepository.CloseFile();
        }
    }
}
