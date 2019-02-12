///////////////////////////////////////////////////////////////////////
// DepAnal.cs - mainly for dependency analysis                       //
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
 *   TypeAnal  - findes, for each file in a specified collection, all other files
 *               from the collection on which they depend. File A depends on file B,
 *               if and only if, it uses the name of any type defined in file B.
 *               It might do that by calling a method of a type bt inheriting the
 *               type. Note that this intentionally does not record dependencies of
 *               a file on files outside that file set, e.g., language and platform
 *               libraries.
 * This module has the following classes
 *    DepAnal - mainly for dependency analysis
 *    TestDepAnal- mainly for test the function of dependency analysis
 *   
 * Three functions:
 *   void show(); - mainly for show the result of dependency analyzing
 */
/* Required Files:
 *   IRulesAndActions.cs, RulesAndActions.cs, ITokenCollections.cs, Parser.cs,
 *   Semi.cs, Toker.cs, ScopStack.cs, TypeTable.cs, Display.cs, DepAnal.cs
 *   TypeAnal.cs, 
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
    class DepAnal {
        public TypeTable table = new TypeTable();
        public List<Elem_relation> relation = new List<Elem_relation>();
        public List<string> class_relation = new List<string>();
        public List<string> composed_relation = new List<string>();

        public DepAnal() {
            relation = Repository.getInstance().relation_;
            class_relation = Repository.getInstance().class_relation_;
            composed_relation = Repository.getInstance().composed_relation_;
            table = Repository.getInstance().table_;
        }
        public void show(List<Elem_relation> relations) {
            foreach (var it in relations)
            {
                Console.Write(it.type1);
                Console.Write(it.name1);
                Console.Write(it.file1);
                Console.Write("->");
                Console.Write(it.type2);
                Console.Write(it.name2);
                Console.Write(it.file2);
                Console.Write("    relation    ");
                Console.Write(it.relation);
                Console.Write("\n");
            }
        }
    }
    class TestDepAnal
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

        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrating DepAnal");
            Console.Write("\n ======================\n");

            ShowCommandLine(args);
            List<string> files = TestDepAnal.ProcessCommandline(args);
            List<string> composed_rela = new List<string>();
            List<Elem> local_location = new List<Elem>();
            Dictionary<File, List<Elem>> table  = new Dictionary<File, List<Elem>>();
            BuildCodeAnalyzer_relation builder_relation;
            BuildCodeAnalyzer builder;
            Parser parser;
            Parser parser_;
            for (int i = 0; i < 2; i++)
            {
                foreach (string file in files)
                {
                    Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));
                    ITokenCollection semi_relation = Factory.create();
                    ITokenCollection semi = Factory.create();

                    if (!semi.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", args[0]);
                        return;
                    }
                    if (!semi_relation.open(file as string))
                    {
                        Console.Write("\n  Can't open {0}\n\n", args[0]);
                        return;
                    }

                    if (i == 0)
                    {
                        builder = new BuildCodeAnalyzer(semi);
                        parser = builder.build();
                        Repository rep1 = Repository.getInstance();
                        rep1.setfileName(Path.GetFileName(file.ToString()));
                        rep1.locations.Clear();
                        try
                        {
                            while (semi.get().Count > 0)
                                parser.parse(semi);

                            if (rep1.composed_relation_.Count() != 0)
                            {
                                for (int num = 0; num < rep1.composed_relation_.Count(); num++)
                                    composed_rela.Add(rep1.composed_relation_[num]);
                            }
                            if (rep1.locations_.Count() != 0) {
                                for (int num = 0; num < rep1.locations_.Count(); num++)
                                    local_location.Add(rep1.locations_[num]);
                            }
                            if (!table.ContainsKey(file)) {
                                table.Add(file, new List<Elem>());
                            }
                            table[file] = local_location;
                        }
                        catch (Exception ex)
                        {
                            Console.Write("\n\n  {0}\n", ex.Message);
                        }                       
                    }
                    else
                    {
                        builder_relation = new BuildCodeAnalyzer_relation(semi_relation);
                        parser_ = builder_relation.build_relation();
                        Repository rep1 = Repository.getInstance();
                        rep1.setfileName(Path.GetFileName(file.ToString()));
                        //rep1.locations.Clear();
                        rep1.composed_relation_ = composed_rela;
                        rep1.locations_ = table[file];
                        try
                        {
                            while (semi_relation.get().Count > 0)
                                parser_.parse(semi_relation);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("\n\n  {0}\n", ex.Message);
                        }
                        Console.Write("the result of DepAnal: \n");
                        DepAnal test = new DepAnal();
                        test.show(rep1.relations);                      
                    }
                    semi.close();
                }
            }
        }
    }   
}
