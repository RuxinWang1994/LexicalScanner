///////////////////////////////////////////////////////////////////////
// TypeAnal.cs - finds all the types defined in each of a collection //
//               of c# source files.                                 //
// ver 1.5                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: Homework for CSE681, Project #3, Fall 2018           //
// Author:      Ruxin (daisy) Wang, Syracuse University              //
//              (315) 450-2106, rwang64@syr.edu                      //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   TypeAnal  - findes all the types defined in each of a collection of c# source files.
 *               It does this by building rules to detect type definitions- classes,
 *               struct, enums, and aliases.
 *   
 * Three functions:
 *   void Save(); - mainly for retrive the data stored in the prcess of parsing,
 *                  and then save the needed info in TypeTable.
 *   void show(); - mainly for show the result of type analyzing
 */
/* Required Files:
 *   IRulesAndActions.cs, RulesAndActions.cs, ITokenCollections.cs, Parser.cs,
 *   Semi.cs, Toker.cs, parser.cs, ScopStack.cs, TypeTable.cs, Display.cs
 *   TypeAnal.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.5 : 14 Oct 2014
 * - added bug fix to tokenizer to avoid endless loop on
 *   multi-line strings
 * ver 1.4 : 30 Sep 2014
 * - modified test stub to display scope counts
 * ver 1.3 : 24 Sep 2011
 * - Added exception handling for exceptions thrown while parsing.
 *   This was done because Toker now throws if it encounters a
 *   string containing @".
 * - RulesAndActions were modified to fix bugs reported recently
 * ver 1.2 : 20 Sep 2011
 * - removed old stack, now replaced by ScopeStack
 * ver 1.1 : 11 Sep 2011
 * - added comments to parse function
 * ver 1.0 : 28 Aug 2011
 * - first release
 */


using Lexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    using File = String;
    class TypeAnal
    {

        public List<Elem> class_Type = new List<Elem>();
        public string pGlobalNameSpace_;
        public string filename_;
        public TypeItem records_ = new TypeItem();
        public TypeTable table_ = new TypeTable();

        public TypeAnal()
        {
            class_Type = Repository.getInstance().class_type_;
            pGlobalNameSpace_ = Repository.getInstance().pGlobalNameSpace_;
            filename_ = Repository.getInstance().filename_;
            table_ = Repository.getInstance().table_;
        }

        public void Save(List<Elem> types, string pGlobal, string filename)
        {
            foreach (var type in types)
            {
                records_.namesp = pGlobal;
                records_.file = filename;
                if (!table_.table.ContainsKey(type.name))
                {
                    table_.table.Add(type.name, new List<TypeItem>());
                }
                table_.table[type.name].Add(records_);
            }
        }

        public void show(TypeTable table)
        {
            table.show();
        }

        class TestTypeAnal
        {
            //----< process commandline to get file references >-----------------

            static List<string> ProcessCommandline(string[] args)
            {
                List<string> files = new List<string>();
                if (args.Length == 0)
                {
                    Console.Write("\n  Please enter file(s) to analyze\n\n");
                    return files;
                }
                string path = args[0];
                path = Path.GetFullPath(path);
                for (int i = 1; i < args.Length; ++i)
                {
                    string filename = Path.GetFileName(args[i]);
                    files.AddRange(Directory.GetFiles(path, filename));
                }
                return files;
            }

            static void ShowCommandLine(string[] args)
            {
                Console.Write("\n  Commandline args are:\n  ");
                foreach (string arg in args)
                {
                    Console.Write("  {0}", arg);
                }
                Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
                Console.Write("\n");
            }

        //    static void Main(string[] args)
        //    {
        //        Console.Write("\n  Demonstrating Parser");
        //        Console.Write("\n ======================\n");

        //        ShowCommandLine(args);

        //        List<string> files = TestTypeAnal.ProcessCommandline(args);
        //        foreach (string file in files)
        //        {
        //            Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

        //            ITokenCollection semi = Factory.create();
        //            //semi.displayNewLines = false;
        //            if (!semi.open(file as string))
        //            {
        //                Console.Write("\n  Can't open {0}\n\n", args[0]);
        //                return;
        //            }
        //            Console.Write("\n  Type and Function Analysis");
        //            Console.Write("\n ----------------------------");

        //            BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
        //            Parser parser = builder.build();

        //            try
        //            {
        //                while (semi.get().Count > 0)
        //                    parser.parse(semi);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.Write("\n\n  {0}\n", ex.Message);
        //            }
        //            Repository rep = Repository.getInstance();
        //            rep.setfileName(file);
        //            TypeAnal typeAnal = new TypeAnal();
        //            typeAnal.Save(rep.class_type_, rep.pGlobalNameSpace_, rep.filename);
        //            rep.table_ = typeAnal.table_;
        //            Console.Write("typetable: ");
        //            Console.Write("\n");
        //            rep.table_.show();
        //            Console.Write("\n");

        //            semi.close();
        //        }
        //        Console.Write("\n\n");
        //    }
        }
    }
}
