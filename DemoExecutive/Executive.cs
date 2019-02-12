///////////////////////////////////////////////////////////////////////
// Executive.cs - Demonstrate Prototype Code Analyzer                //
// ver 1.0                                                           //
// Language:    C#, 2017, .Net Framework 4.7.1                       //
// Application: Homework for CSE681, Project #3, Fall 2018           //
// Author:      Ruxin (daisy) Wang, Syracuse University              //
//              (315) 450-2106, rwang64@syr.edu                      //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines the following class:
 *   Executive:
 *   - uses Parser, RulesAndActions, Semi, and Toker to perform basic
 *     code metric analyzes
 *     
 * This module defines one class
 *    Executive - mainly for demo meet the requirement
 *    
 *    
 * There are five functions in this module
 *   List<string> ProcessCommandline(string[] args)
 *   void ShowCommandLine(string[] args)
 *   void testParser(string[] args)
 *   void testTypeAnal(string[] args)
 *   void testDepAnal(string[] args)
 *   
 */
/* Required Files:
 *   Executive.cs
 *   Parser.cs
 *   IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 *   ITokenCollection.cs, Semi.cs, Toker.cs
 *   Display.cs, TypeAnal.cs, DepAnal.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 09 Oct 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
  using File = String;
  using Lexer;

  class Executive
  {
        //----< process commandline to get file references >-----------------

        static List<string> ProcessCommandline(string[] args)
        {
            List<string> files = new List<string>();
            if (args.Length < 2)
            {
                Console.Write("\n  Please enter path and file(s) to analyze\n\n");
                return files;
            }
            string path = args[0];
            if (!Directory.Exists(path))
            {
                Console.Write("\n  invalid path \"{0}\"", System.IO.Path.GetFullPath(path));
                return files;
            }
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

        public void testParser(string[] args) {
            Console.Write("\n  Demonstrating Parser");
            Console.Write("\n ========================  \n");

            ShowCommandLine(args);

            List<string> files = ProcessCommandline(args);
            foreach (string file in files)
            {
                Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

                ITokenCollection semi = Factory.create();
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }
                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                rep.setfileName(file);
                Console.Write(rep.filename_);
                Console.Write(rep.pGlobalNameSpace_);
                List<Elem> class_ = rep.class_type_;
                Console.Write("\n");
                Console.Write("stack: \n");
                rep.stack.display();

                Console.Write("\n");
                Console.Write("tree: \n");
                foreach (var it in rep.class_type_)
                {
                    Console.Write("{0},{1}/n", it.name, it.type);
                }

                Display.showMetricsTable(class_);
                Console.Write("\n");

                semi.close();
            }
            Console.Write("\n\n\n");

        }
        public void testTypeAnal(string[] args) {
            Console.Write("\n  Demonstrating TypeAnal");
            Console.Write("\n ========================  \n");
            ShowCommandLine(args);
            List<string> files = ProcessCommandline(args);
            foreach (string file in files)
            {
                Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

                ITokenCollection semi = Factory.create();
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }
                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                rep.setfileName(file);
                TypeAnal typeAnal = new TypeAnal();
                typeAnal.Save(rep.class_type_, rep.pGlobalNameSpace_, rep.filename);
                rep.table_ = typeAnal.table_;
                Console.Write("typetable: ");
                Console.Write("\n");
                rep.table_.show();
                Console.Write("\n");

                semi.close();
            }
            Console.Write("\n\n\n");
        }

        public void testDepAnal(string[] args) {
            Console.Write("\n  Demonstrating DepAnal");
            Console.Write("\n ========================  \n");

            ShowCommandLine(args);
            List<string> files = ProcessCommandline(args);
            List<string> composed_rela = new List<string>();
            List<Elem> local_location = new List<Elem>();
            Dictionary<File, List<Elem>> table = new Dictionary<File, List<Elem>>();
            BuildCodeAnalyzer_relation builder_relation;
            BuildCodeAnalyzer builder;
            Parser parser;
            Parser parser_;
            for (int i1 = 0; i1 < 2; i1++)
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

                    if (i1 == 0)
                    {
                        builder = new BuildCodeAnalyzer(semi);
                        parser = builder.build();
                        Repository rep1 = Repository.getInstance();
                        rep1.setfileName(Path.GetFileName(file.ToString()));
                        try
                        {
                            while (semi.get().Count > 0)
                                parser.parse(semi);

                            if (rep1.composed_relation_.Count() != 0)
                            {
                                for (int num = 0; num < rep1.composed_relation_.Count(); num++)
                                    composed_rela.Add(rep1.composed_relation_[num]);
                            }
                            if (rep1.locations_.Count() != 0)
                            {
                                for (int num = 0; num < rep1.locations_.Count(); num++)
                                    local_location.Add(rep1.locations_[num]);
                            }
                            if (!table.ContainsKey(file))
                            {
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
                        rep1.composed_relation_ = composed_rela;
                        if (!table.ContainsKey(file))
                        {
                            table.Add(file, new List<Elem>());
                        }
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

        static void Main(string[] args)
        {
            Console.Write("meet the requirement#1 \n");
            Console.Write("================================================== \n");
            Console.Write("Use Visual Studio 2017 and its C# Windows Console Projects, as provided in the ECS computer labs. \n\n\n");

            Console.Write("meet the requirement#2 \n");
            Console.Write("================================================== \n");
            Console.Write("use the .Net System.IO and System.Text for all I/O. \n\n\n");

            Console.Write("meet the requirement#3 \n");
            Console.Write("================================================== \n");
            Console.Write("provide C# packages :Toker,SemiExp,TypeTable,TypeAnal,DepAnal,StrongComponent,Display,DemoExecutive(Tester). \n\n\n");

            Console.Write("meet the requirement#4 \n");
            Console.Write("================================================== \n");
            Executive test = new Executive();
            test.testParser(args);
            test.testTypeAnal(args);
            test.testDepAnal(args);
            Console.Write("\n\n\n");

            Console.Write("meet the requirement#5 \n");
            Console.Write("================================================== \n");
            test.testDepAnal(args);
            Console.Write("\n\n\n");

            Console.Write("meet the requirement#6 \n");
            Console.Write("================================================== \n");
            Console.Write("not finished \n\n\n");

            Console.Write("meet the requirement#7 \n");
            Console.Write("================================================== \n");
            Console.Write("Shall display the results in a well formated area of the output. \n\n\n");

            Console.Write("meet the requirement#8 \n");
            Console.Write("================================================== \n");
            Console.Write("Include an automated unit test suite that demonstrates the requirements you've implemented and exercises all of the special cases that seem appropriate for these two packages.");
        }
    }
}
