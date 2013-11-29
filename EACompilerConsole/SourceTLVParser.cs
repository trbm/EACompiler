using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EACompilerII
{
    class SourceTLVParser
    {
        private List<List<MatchRecord>> _MatchRecordFileList = new List<List<MatchRecord>>();

        public static string FILE_EXTENSION_ADDITION_XML = ".TLV.xml";

        private const string FILE_EXTENSION_ADDITION_H = ".TLV.h";
        private const string FILE_EXTENSION_ADDITION_C = ".TLV.c";

        private bool DumpFileContents;
        private bool ShowFullyQualifiedFilenames;

        public SourceTLVParser(string sRootSearchDirectory, bool bShowFullyQualifiedFilenames, bool bDumpFileContents)
        {
            DumpFileContents = bDumpFileContents;
            ShowFullyQualifiedFilenames = bShowFullyQualifiedFilenames;
            DirectoryInfo oRootSearchDirectoryInfo = new DirectoryInfo(sRootSearchDirectory);
            Console.WriteLine("    Scanning for foreground TLVs: " + oRootSearchDirectoryInfo.FullName);
            recurseDirectories(oRootSearchDirectoryInfo.FullName);
            Console.WriteLine("");
            createXMLfiles();
            createIncludeFiles_C();
            createIncludeFiles_H();
        }

        private void createIncludeFiles_C()
        {
            foreach (List<MatchRecord> oMatchRecordList in _MatchRecordFileList)
            {
                if (oMatchRecordList.Count > 0)
                {
                    string sFullyQualifiedFilename = oMatchRecordList[0].FullyQualifiedFilename;
                    string sFilenameBase = oMatchRecordList[0].FilenameBase;
                    string sFilenameRoot = Path.GetFileName(sFullyQualifiedFilename);
                    OutputFile oOutputFile = new OutputFile(sFullyQualifiedFilename + FILE_EXTENSION_ADDITION_C);
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY");
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //           Filename: " + oOutputFile.FileInfo.Name);
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //          Generated: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString());
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //            © Copyright 2012,  ArrayPower Inc.   All rights reserved.");
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add("");
                    oOutputFile.Add("#include \"" + sFilenameRoot + ".TLV.H\"");
                    oOutputFile.Add("");
                    oOutputFile.Add("static TLV_TUPLE TLV_TABLE[ " + oMatchRecordList.Count.ToString() + " + 1 ];");
                    oOutputFile.Add("void InitializeTLVs_" + sFilenameBase + "()      ");
                    oOutputFile.Add("{");
                    oOutputFile.Add("    int i = 0;   // incrementing array index");
                    oOutputFile.Add("");
                    foreach (MatchRecord oMatchRecord in oMatchRecordList)
                    {
                        oOutputFile.Add("    SetTLVtuple( &TLV_TABLE[i++], " + oMatchRecord.TagId + ", tlvType_" + oMatchRecord.TypeSpecifier + ", &(" + oMatchRecord.Name + "), NeverPersist,  null, null);");
                    }
                    oOutputFile.Add("");
                    oOutputFile.Add("    TLV_TUPLE _terminator = { 0, tlvType_terminator_zero, 0 };  TLV_TABLE[i++] = _terminator;   // last entry of zeros terminates the table");
                    oOutputFile.Add("");
                    oOutputFile.Add("    RegisterDomainTLVlist( TLV_TABLE );                         // add this TLV table to the list of TLV tables");
                    oOutputFile.Add("};");
                    oOutputFile.Add("");
                    oOutputFile.Close();
                }
            }
        }

        private void createIncludeFiles_H()
        {
            foreach (List<MatchRecord> oMatchRecordList in _MatchRecordFileList)
            {
                if (oMatchRecordList.Count > 0)
                {
                    string sFullyQualifiedFilename = oMatchRecordList[0].FullyQualifiedFilename;
                    string sFilenameRoot = Path.GetFileName(sFullyQualifiedFilename);
                    OutputFile oOutputFile = new OutputFile(sFullyQualifiedFilename + FILE_EXTENSION_ADDITION_H);
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY");
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //           Filename: " + oOutputFile.FileInfo.Name);
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //          Generated: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString());
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add(" //");
                    oOutputFile.Add(" //            © Copyright 2012,  ArrayPower Inc.   All rights reserved.");
                    oOutputFile.Add(" // ____________________________________________________________________________");
                    oOutputFile.Add("");
                    foreach (MatchRecord oMatchRecord in oMatchRecordList)
                    {
                        oOutputFile.Add("#define " + oMatchRecord.TagId + " " + oMatchRecord.HashcodeString);
                    }
                    oOutputFile.Add("");
                    oOutputFile.Close();
                }
            }
        }

        private void recurseDirectories(string sDirectoryPath)
        {
            foreach (string sDirectory in Directory.GetDirectories(sDirectoryPath))
            {
                if (!sDirectory.Contains(".svn"))
                {
                    Console.WriteLine("       " + sDirectory);
                    foreach (string sFilename in Directory.GetFiles(sDirectory, "*.c"))
                    {
                        parseFile(sFilename);
                    }
                    recurseDirectories(sDirectory);
                }
            }
        }

        private void parseFile(string sFilename)
        {
            if (ShowFullyQualifiedFilenames)
            {
                Console.WriteLine(sFilename);
            }

            InputFile oInputFile = new InputFile(sFilename);
            if (DumpFileContents)
            {
                Console.WriteLine("____________________________________________________");
                Console.WriteLine(oInputFile.ToString());
            }
            List<MatchRecord> oMatchRecordList = new List<MatchRecord>();

            parseForTLVtag(sFilename, oInputFile, oMatchRecordList);
            parseForFileInclude(sFilename, oInputFile, oMatchRecordList);
        }

        void parseForFileInclude(string sFilename, InputFile oInputFile, List<MatchRecord> oMatchRecordList)        // destroy any of our old .c, .h, or .xml files in case they are no longer needed
        {
            SmartMatch oSmartMatch = new SmartMatch(oInputFile.ToString(), "#include[ ]+\"([0-9a-zA-Z_]+).c.TLV.C\"");
            if (oSmartMatch.Success)
            {
                OutputFile oEmptyIncludeFile = new OutputFile(sFilename + FILE_EXTENSION_ADDITION_C);
                oEmptyIncludeFile.AddLine("// this is an automatically generated file -- an empty file to satisfy a possible #include");
                oEmptyIncludeFile.AddLine("void InitializeTLVs_" + oSmartMatch.get_MatchGroup(0, 1) + "()  {}");
                oEmptyIncludeFile.Close();

                oEmptyIncludeFile = new OutputFile(sFilename + FILE_EXTENSION_ADDITION_H);
                oEmptyIncludeFile.AddLine("// this is an automatically generated file -- an empty file to satisfy a possible #include");
                oEmptyIncludeFile.Close();

                oEmptyIncludeFile = new OutputFile(sFilename + FILE_EXTENSION_ADDITION_XML);
                oEmptyIncludeFile.AddLine("<!-- this is an automatically generated file: an empty file to satisfy a possible #include -->  ");
                oEmptyIncludeFile.AddLine("<empty/>");
                oEmptyIncludeFile.Close();
            }
        }

        private void parseForTLVtag(string sFilename, InputFile oInputFile, List<MatchRecord> oMatchRecordList)
        {
            string commentlessFileString = Regex.Replace(oInputFile.ToString(), "//.+;", "");           // remove any commented out lines (that end in ';')
            SmartMatch oSmartMatch = new SmartMatch(commentlessFileString, "([a-zA-z0-9_]*)[ ]*TLV[ ]+([a-zA-z0-9_]*)[ ]+([a-zA-z0-9_]*)[ ]+([a-zA-z0-9_]*)");
            if (oSmartMatch.Success)
            {
                for (int i = 0; i < oSmartMatch.Matches.Count; i++)
                {
                    string sCandidateTypeSpecifier = oSmartMatch.get_MatchGroup(i, 1);
                    if (Canonical.IsCanonicalType(sCandidateTypeSpecifier))     // check for a preceding type specifier
                    {
                        oMatchRecordList.Add(new MatchRecord(sFilename, sCandidateTypeSpecifier, oSmartMatch.get_MatchGroup(i, 2)));
                    }

                    sCandidateTypeSpecifier = oSmartMatch.get_MatchGroup(i, 2);
                    if (Canonical.IsCanonicalType(sCandidateTypeSpecifier))     // check for a following type specifier
                    {
                        oMatchRecordList.Add(new MatchRecord(sFilename, sCandidateTypeSpecifier, oSmartMatch.get_MatchGroup(i, 3)));
                    }

                    sCandidateTypeSpecifier = oSmartMatch.get_MatchGroup(i, 3);
                    if (Canonical.IsCanonicalType(sCandidateTypeSpecifier))     // check for TLV followed by 'static' then a type specifier
                    {
                        oMatchRecordList.Add(new MatchRecord(sFilename, sCandidateTypeSpecifier, oSmartMatch.get_MatchGroup(i, 4)));
                    }
                }
            }
            if (oMatchRecordList.Count > 0)
            {
                _MatchRecordFileList.Add(oMatchRecordList);
            }
        }

        private void createXMLfiles()
        {
            foreach (List<MatchRecord> oMatchRecordList in _MatchRecordFileList)
            {
                if (oMatchRecordList.Count > 0)
                {
                    if (ShowFullyQualifiedFilenames)
                    {
                        Console.Write("        " + Path.GetFileName(oMatchRecordList[0].FullyQualifiedFilename) + "\n");
                    }

                    OutputFile oOutputFile = new OutputFile(oMatchRecordList[0].FullyQualifiedFilename + FILE_EXTENSION_ADDITION_XML);
                    oOutputFile.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    oOutputFile.Add("<!--");
                    oOutputFile.Add(" ** ____________________________________________________________________________");
                    oOutputFile.Add(" **");
                    oOutputFile.Add(" **          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY");
                    oOutputFile.Add(" ** ____________________________________________________________________________");
                    oOutputFile.Add(" **");
                    oOutputFile.Add(" **           Filename: " + oOutputFile.FileInfo.Name);
                    oOutputFile.Add(" **");
                    oOutputFile.Add(" **          Generated: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString());
                    oOutputFile.Add(" ** ____________________________________________________________________________");
                    oOutputFile.Add(" **");
                    oOutputFile.Add(" **            © Copyright 2012,  ArrayPower Inc.   All rights reserved.");
                    oOutputFile.Add(" ** ____________________________________________________________________________");
                    oOutputFile.Add("-->");
                    oOutputFile.Add("<root xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" >");
                    oOutputFile.Add("    <TagLlinkValues>");
                    foreach (MatchRecord oMatchRecord in oMatchRecordList)
                    {
                        Console.Write("            " + oMatchRecord.TypeSpecifier + " " + oMatchRecord.Name + "\n");
                        oOutputFile.Add("    <TagLinkValue tagId=\"" + oMatchRecord.TagId + "\" domain=\"" + oMatchRecord.Filename + "\"    class=\"\"  attribute=\"" + oMatchRecord.Name + "\"     attributeType=\"" + oMatchRecord.TypeSpecifier + "\"    hashcode=\"" + oMatchRecord.HashcodeString + "\"   persistent=\"NeverPersist\"  description=\"(foreground TLV)\"    />");
                    }
                    oOutputFile.Add("    </TagLlinkValues>");
                    oOutputFile.Add("</root>");
                    oOutputFile.Add("");
                    oOutputFile.Close();
                }
            }
        }

        private class MatchRecord
        {
            public string FullyQualifiedFilename { get; set; }
            public string Filename { get; set; }
            public string FilenameBase { get; set; }
            public string TypeSpecifier { get; set; }
            public string Name { get; set; }
            public string TagId { get; set; }
            public long Hashcode { get; set; }
            public string HashcodeString { get; set; }

            public MatchRecord(string sFullyQualifiedFilename, string sTypeSpecifier, string sName)
            {
                FullyQualifiedFilename = sFullyQualifiedFilename;
                Filename = Path.GetFileName(sFullyQualifiedFilename);
                FilenameBase = Filename.Substring(0, Filename.IndexOf("."));
                TypeSpecifier = sTypeSpecifier;
                Name = sName;
                TagId = Regex.Replace(Canonical.CanonicalName(Filename + "_" + TypeSpecifier + "_" + Name), "\\.", "_");
                Hashcode = TagId.GetHashCode();
                HashcodeString = Hashcode.ToString();
            }
        }
    }
}
