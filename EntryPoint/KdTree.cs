using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryPoint {

    class KdTree {
        private const int DIMENSION = 2;
        private const int X = 0;
        private const int Y = 1;

        public Node root;

        public KdTree() {
            root = null;
        }

        public bool IsEmpty() {
            return root == null;
        }

        public void Insert(Vector2 v) {
            if (IsEmpty()) {
                root = new Node(v);
            }
            else {
                root.Insert(ref root, v, 0);
            }
            
        }

        public List<Vector2> preOrderTraversal(Node n) {
            if (n == null) {
                return null;
            }

            Stack<Node> nodeStack = new Stack<Node>();
            List<Vector2> allNodes = new List<Vector2>();
            nodeStack.Push(root);

            while (nodeStack.Count > 0) {
                Node node = nodeStack.Peek();
                allNodes.Add(node.value);
                nodeStack.Pop();

                if(node.right != null) {
                    nodeStack.Push(node.right);
                }
                if(node.left != null) {
                    nodeStack.Push(node.left);
                }
            }
            return allNodes;

        }

        internal class Node {
            public Vector2 value;
            public Node left;
            public Node right;

            public Node(Vector2 v) {
                this.value = v;
                this.left = null;
                this.right = null;
            }

            public bool IsLeaf(Node n) {
                return (n.left == null && n.right == null);
            }

            public void Insert(ref Node n, Vector2 v, int level) {
                int XorY = level % DIMENSION;

                if (n == null) {
                    n = new Node(v);
                }
                else if (XorY == X) {
                    if (v.X <= n.value.X) {
                        Insert(ref n.left, v, level + 1);
                    }
                    else {
                        Insert(ref n.right, v, level + 1);
                    }
                }
                else if (XorY == Y) {
                    if (v.Y <= n.value.Y) {
                        Insert(ref n.left, v, level + 1);
                    }
                    else {
                        Insert(ref n.right, v, level + 1);
                    }
                }
            }
        }
    }
}
