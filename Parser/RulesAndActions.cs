///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.3                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: Homework for CSE681, Project #3, Fall 2018           //
// Author:      Ruxin (daisy) Wang,  Syracuse University             //
//              (315) 450-2016, rwang64@syr.edu                      //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following nine rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectStruct rule
 *   - DetectAlias rule
 *   - DetectEnum rule
 *   - DetectDelegate rule
 *   - DetectFunction rule
 *   - DetectAnonymousScope rule
 *   - DetectLeavingScope rule
 *   
 *   Three actions - some are specific to a parent rule:
 *   - PushStack
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, ITokenCollection.cs,
 *   Parser.cs, ScopeStack.cs, Display.cs, TypeTable.cs
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.4 : 09 Oct 2018
 * - modified comments
 * - removed unnecessary definition from repository class
 * - moved local semi definition inside display test in PopStack action
 * ver 2.3 : 30 Sep 2014
 * - added scope-based complexity analysis
 *   Note: doesn't detect braceless scopes yet
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lexer;

namespace CodeAnalysis
{
    using Package = String;
    using Path = String;
    ///////////////////////////////////////////////////////////////////
    // Repository class
    // - Specific to each application
    // - holds results of processing
    // - ScopeStack holds current state of scope processing
    // - List<Elem> holds start and end line numbers for each scope
    ///////////////////////////////////////////////////////////////////
    
    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }
        public string filename { get; set; }
        public int beginLine { get; set; }
        public int endLine { get; set; }
        public int beginScopeCount { get; set; }
        public int endScopeCount { get; set; }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-10}", filename)).Append(" : ");
            temp.Append(String.Format("{0,-5}", beginLine.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-5}", endLine.ToString()));    // line of scope end
            temp.Append("}");
            return temp.ToString();
        }
    }
    public class Elem_relation
    {
        public string file1 { get; set; }
        public string type1 { get; set; }
        public string name1 { get; set; }
        public string file2 { get; set; }
        public string type2 { get; set; }
        public string name2 { get; set; }
        public string relation { get; set; }

    }

    class Repository
    {
        public ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        public String filename_;
        public String pGlobalNameSpace_;
        public List<Elem> class_type_ = new List<Elem>();
        public List<Elem> locations_ = new List<Elem>();
        public TypeTable table_ = new TypeTable();
        public List<Elem_relation> relation_ = new List<Elem_relation>();
        public List<string> class_relation_ = new List<string>();
        public List<string> composed_relation_ = new List<string>();

        public static Repository instance;

        public String filename { get { return filename_; }  }
        public void setfileName(String name) { filename_ = name; }

        public Repository() {
            instance = this;
        }

        //----< provides all code access to Repository >-------------------
        public static Repository getInstance()
        {
            return instance;
        }
        //----< provides all actions access to current semiExp >-----------
        public ITokenCollection semi
        {
            get;
            set;
        }     

        public ScopeStack<Elem> stack { get { return stack_; } }
        public List<Elem> locations { get { return locations_; } }
        public List<Elem_relation> relations
        {
            get { return relation_; }
        }
        public List<string> class_relation { get { return class_relation_; } }
        public List<string> composed_relations
        {
            get { return composed_relation_; }
        }
        public TypeTable table { get { return table_; } }
    }
    ///////////////////////////////////////////////////////////////////
    // Define Actions
    ///////////////////////////////////////////////////////////////////
    // - PushStack
    // - PopStack
    // - PrintFunction
    // - PrintSemi
    // - SaveDeclar
    /////////////////////////////////////////////////////////
    // pushes types relations info on stack when entering new scope
    ///////////////////////////////////////////////////////////////
    // action to push stack
    class PushStack_relation : AAction
    {
        public PushStack_relation(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action PushStack_relation");
            Elem_relation elem = new Elem_relation();
            if (semi[0] == "class" || semi[0] == "function")
            {
                repo_.class_relation.Add(semi[1]);
                return;
            }
            foreach (Elem baseclass in repo_.locations)
            {
                if (baseclass.name == semi[1])
                {
                    elem.relation = semi[0];
                    elem.type1 = baseclass.type;
                    elem.name1 = baseclass.name;
                    elem.file1 = baseclass.filename;
                    elem.file2 = repo_.filename;
                    if (semi.size() > 2)
                    {
                        elem.type2 = semi[2];
                        elem.name2 = semi[3];
                        repo_.class_relation.Add(semi[3]);
                    }
                    else
                    {
                        elem.name2 = repo_.class_relation[repo_.class_relation.Count - 1];
                        if (repo_.class_relation[repo_.class_relation.Count - 1] == "Main")
                            elem.type2 = "function";
                        else
                            elem.type2 = "class";
                    }
                    repo_.relations.Add(elem);
                    return;
                }
            }
        }
    }
    ///////////////////////////////////////////////////////////////
    // action to push stack
    class PushStack : AAction {
        public PushStack(Repository repo) {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action PushStack");
            Elem elem = new Elem();
            elem.filename = repo_.filename;
            elem.type = semi[0];
            elem.name = semi[1];
            repo_.stack_.push(elem);

            if (elem.type == "struct" || elem.type == "enum")
            {
                repo_.composed_relation_.Add(elem.name);
            }

            if (elem.type == "control" || elem.name == "anonymous") {
                return;
            }
            repo_.locations.Add(elem);
            if (elem.type == "namespace") {
                repo_.pGlobalNameSpace_ = elem.name;
            }

            if (AAction.displayStack)
                repo_.stack_.display();
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack_.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            if (elem.type == "namespace" || elem.type == "interface" || elem.type == "class" || elem.type == "enum" || elem.type == "delegate"||elem.type=="alias") {
                repo_.class_type_.Add(elem);
            }
        }
    }
    ///////////////////////////////////////////////////////////////
    // action to pop stack
    class PopStack : AAction {
        public PopStack(Repository repo) {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Elem elem;
            try
            {
                elem = repo_.stack_.pop();
            }
            catch {
                Console.Write("poped empty stack");
                semi.show();
                return;
            }
            ITokenCollection local = Factory.create();
            local.add(elem.type).add(elem.name);
            if (local[0] == "control")
                return;

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("leaving ");
                string indent = new string(' ', 2 * repo_.stack_.count);
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }

    ///////////////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo
    class PrintFunction : AAction
    {
        public PrintFunction(Repository repo)
        {
            repo_ = repo;
        }
        public override void display(Lexer.ITokenCollection semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount() - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.size(); ++i)
            {
                if (semi[i] != "\n")
                    Console.Write("{0} ", semi[i]);
            }
        }
        public override void doAction(ITokenCollection semi)
        {
            this.display(semi);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // ITokenCollection printing action, useful for debugging
    class PrintSemi : AAction
    {
        public PrintSemi(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount() - 1);
            this.display(semi);
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Define Rules
    ///////////////////////////////////////////////////////////////////
    // - DetectNamespace
    // - DetectClass
    // - DetectFunction
    // - DetectAnonymousScope
    // - DetectPublicDeclaration
    // - DetectLeavingScope    

    ///////////////////////////////////////////////////////////////////
    // rule to detect namespace declarations
    class DetectNamespace : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectNamespace");
            int index;
            semi.find("namespace", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                Repository.getInstance().pGlobalNameSpace_ = semi[index + 1];
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect struct declarations
    class DetectStruct : ARule
    {
        public override bool test(ITokenCollection semi)
        {          
            Display.displayRules(actionDelegate, "rule DetectNamespace");
            int index;
            semi.find("struct", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect enum declarations
    class DetectEnum : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectEnum");
            int index;
            semi.find("enum", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 1]);
                doActions(semi);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect alias declarations
    class DetectAlias : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectEnum");
            int index;
            semi.find("alias", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 1]);
                doActions(semi);
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to detect delegate declarations
    class DetectDelegate : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectDelegate");
            int index;
            semi.find("delegate", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 2]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect class definitions
    class DetectClass : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectClass");
            int indexCL;
            semi.find("class", out indexCL);
            int indexIF;
            semi.find("interface", out indexIF);
            int indexST;
            semi.find("struct", out indexST);

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }    
    ///////////////////////////////////////////////////////////////////
    // rule to dectect function definitions
    class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectFunction");
            if (semi[semi.size() - 1] != "{")
                return false;

            int index;
            semi.find("(", out index);
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                ITokenCollection local = Factory.create();
                local.add("function").add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those
    class DetectAnonymousScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectAnonymousScope");
            int index;
            semi.find("{", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add("control").add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }   
    ///////////////////////////////////////////////////////////////////
    // rule to detect end of scope
    class DetectLeavingScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule DetectLeavingScope");
            int index;
            semi.find("}", out index);
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////////////////
    //// rule to detect inheritance relationship
    class DetectInheritance : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int index;
            semi.find(":", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                local.add("inheritance").add(semi[index + 1]).add(semi[index - 2]).add(semi[index - 1]);
                doActions(local);
                if (semi.contains("."))
                {
                    int index1;
                    semi.find(".", out index1);
                    local.insert(1, semi[index1 + 1]);
                    local.remove(2);
                    doActions(local);
                }
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect ClassRelation relationship
    class DetectClassRelation : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int indexCl;
            semi.find("class", out indexCl);
            int indexMa;
            semi.find("Main", out indexMa);
            int index = Math.Max(indexCl, indexMa);
            index = Math.Max(index, indexMa);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                if (semi[index] == "Main")
                {
                    local.add("function").add(semi[index]);
                }
                else
                {
                    local.add(semi[index]).add(semi[index + 1]);
                }
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect aggregation relationship
    class DetectAggregation : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int index;
            semi.find("new", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                if (semi.contains("."))
                {
                    int index1;
                    semi.find(".", out index1);
                    local.add("Aggregation").add(semi[index1 + 1]);
                }
                else
                {
                    local.add("Aggregation").add(semi[index + 1]);
                }
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect composition relationship
    class DetectComposition : ARule
    {
        Repository repo = Repository.getInstance();
        public override bool test(ITokenCollection semi)
        {
            foreach (var structlist in repo.composed_relations)
            {
                int index;
                semi.find(structlist, out index);
                if (index != -1 && !semi.contains("struct") && !semi.contains("enum"))
                {
                    ITokenCollection local = Factory.create();
                    local.add("Composition").add(semi[index]);
                    doActions(local);
                    return true;
                }
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to detect using relationship
    class DetectUsing : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "Main", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(ITokenCollection semi)
        {
            if (semi[semi.size() - 1] != "{")
                return false;
            int index;
            semi.find("(", out index);
            if (semi[index + 1] == ")")
                return false;
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                ITokenCollection local = Factory.create();
                if (semi.contains("."))
                {
                    int index1;
                    semi.find(".", out index1);
                    local.add("Using").add(semi[index1 + 1]);
                }
                else
                {
                    local.add("Using").add(semi[index + 1]);
                }
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // BuildCodeAnalyzer class
    ///////////////////////////////////////////////////////////////////

    class BuildCodeAnalyzer
    {
        Repository repo = new Repository();

        public BuildCodeAnalyzer(Lexer.ITokenCollection semi)
        {
            repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // false is default

            // action used for namespaces, classes, and functions            
            PushStack push = new PushStack(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            //capture struct info
            DetectStruct detectSt = new DetectStruct();
            detectSt.add(push);
            parser.add(detectSt);

            //capture alias info
            DetectAlias detectAi = new DetectAlias();
            detectAi.add(push);
            parser.add(detectAi);

            //capture enum info
            DetectEnum detectEn = new DetectEnum();
            detectEn.add(push);
            parser.add(detectEn);

            //capture delegate info
            DetectDelegate detectDl = new DetectDelegate();
            detectDl.add(push);
            parser.add(detectDl);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // capture function info
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(push);
            parser.add(detectFN);

            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);

            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            PopStack pop = new PopStack(repo);
            parser.add(leave);

            // parser configured
            return parser;
        }
    }

    //public class BuildCodeAnalyzer_relation
    //{
    //    Repository repo = new Repository();

    //    public BuildCodeAnalyzer_relation(ITokenCollection semi)
    //    {
    //        repo.semi = semi;
    //    }
    //    public virtual Parser build_relation()
    //    {
    //        Parser parser_rel = new Parser();

    //        // decide what to show
    //        AAction.displaySemi = false;
    //        AAction.displayStack = false;  // this is default so redundant

    //        //// action used for namespaces, classes, and functions
    //        PushStack_relation push_rel = new PushStack_relation(repo);

    //        // capture inheritence info
    //        DetectInheritance detectIn = new DetectInheritance();
    //        detectIn.add(push_rel);
    //        parser_rel.add(detectIn);

    //        // capture inheritence info
    //        DetectAggregation detectAg = new DetectAggregation();
    //        detectAg.add(push_rel);
    //        parser_rel.add(detectAg);

    //        // capture using info
    //        DetectUsing detectUs = new DetectUsing();
    //        detectUs.add(push_rel);
    //        parser_rel.add(detectUs);

    //        // capture using info
    //        DetectComposition detectCm = new DetectComposition();
    //        detectCm.add(push_rel);
    //        parser_rel.add(detectCm);

    //        // capture inheritence info
    //        DetectClassRelation detectCl = new DetectClassRelation();
    //        detectCl.add(push_rel);
    //        parser_rel.add(detectCl);


    //        // parser configured
    //        return parser_rel;

    //    }
    //}

}

