using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    using Package = String;
    using Type = String;
    using Name = String;
    using Path = String;
    using ClassName = String;
    using TypeMap = Dictionary<String, ASTNode>;

    enum Access { publ, prot, priv};
    enum DeclType { dataDecl, functionDecl, lambdaDecl, usingDecl};

    struct DeclarationNode {                
        public Access access_;
        public DeclType declType_;
        public Package package_;
        public Lexer.ITokenCollection semi;
    }

    class ASTNode {
        public Type type_;
        public Type parentType_;
        public Name name_;
        public Path path_;
        public Package package_;
        public bool visited;
        public List<ASTNode> children_;
        public List<DeclarationNode> decl_;    
        public List<Lexer.ITokenCollection> statements_;               
 
        //default constructor
        public ASTNode() {
            type_ = "anonymous";
            parentType_ = "namespace";
            name_ = "none";
            visited = false;
            children_ = new List<ASTNode>();
            decl_ = new List<DeclarationNode>();
            statements_ = new List<Lexer.ITokenCollection>();            
        }
        //constructor
        public ASTNode(Type type, Name name) {
            type_ = type;
            parentType_ = "namespace";
            name_ = name;
            visited = false;
            children_ = new List<ASTNode>();
            decl_ = new List<DeclarationNode>();
            statements_ = new List<Lexer.ITokenCollection>();
        }
        //show ASTNode
        public void show(bool details) {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type_)).Append(" : ");
            temp.Append(String.Format("{0,-10}", parentType_)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name_)).Append(" : ");
            if (details) {
                temp.Append("num children");
                temp.Append(String.Format("{0,-10}", children_.Count)).Append(" : ");
                temp.Append("num statements");
                temp.Append(String.Format("{0,-10}", statements_.Count)).Append(" : ");
            }
            temp.Append("}");
            Console.Write("{0}", temp.ToString());
        }
    }

    class AbstrSynTree {
        public TypeMap typeMap_;
        public ScopeStack<ASTNode> stack_;
        public ASTNode pGolobalNameSpace_;
        
        //default constructor
        public AbstrSynTree(ScopeStack<ASTNode> stack) {
            stack_ = new ScopeStack<ASTNode>();
            typeMap_ = new Dictionary<ClassName, ASTNode>();
            pGolobalNameSpace_ = new ASTNode("namespace", "Global NameSpace");
            stack_ = stack;
            stack_.push(pGolobalNameSpace_);
        }

        public ASTNode root() {
            return pGolobalNameSpace_;
        }

        public TypeMap typeMap() {
            return typeMap_;
        }
        //add ASTNode ptr to stack top element's children and push
        public void add(ASTNode pNode) {
            pNode.parentType_ = stack_[(stack_.count)-1].type_;
            stack_[(stack_.count) - 1].children_.Add(pNode);
            stack_.push(pNode);
            if (pNode.type_.Equals("class") || pNode.type_.Equals("struct") || pNode.type_.Equals("interface"))
            {
                typeMap_[pNode.name_] = pNode;
            }
        }
        //pop stack's top element
        public ASTNode pop(){
            return stack_.pop();
        }
        //goes through the whole tree recursively to find the required node
        public ASTNode findRecursively(ClassName classname, ASTNode pNode) {
            if (pNode.name_ == classname)
                return pNode;
            foreach (var node in pNode.children_){
                if (node.name_ == classname){
                    return node;
                }
            }
            foreach (var node in pNode.children_){
                return findRecursively(classname, node);
            }
            return null;
        }
        //find a type node using typeMap
        public ASTNode find(ClassName classname) {
            foreach (KeyValuePair<ClassName, ASTNode> iter in typeMap_){
                if (iter.Key == classname){
                    return iter.Value;
                }
            }
            return findRecursively(classname, pGolobalNameSpace_);
        }
        //traverse AST
        public void walk() {
            ASTNode root = this.root();
            if (root.children_.Count == 0) {
                Console.Write("\n no nodes in graph");
                return;
            }
            if (root == null) {
                Console.Write("\n no starting node defined");
                return;
            }
            walk(root);
            foreach (var node in root.children_) {
                if (!node.visited) {
                    walk(node);
                }
            }
            foreach (var node in root.children_) {
                node.visited = false;
            }
            return;
        }
        //DFS
        public void walk(ASTNode node) {
            if (node == null)
                return;
            Stack<ASTNode> stack = new Stack<ASTNode>();
            stack.Push(node);
            node.visited = true;
            while (stack.Count != 0) {
                ASTNode cur = stack.Pop();
                foreach (var next in cur.children_) {
                    stack.Push(cur);
                    stack.Push(next);
                    next.visited = true;
                    break;
                }
            }
        }
    }
}
