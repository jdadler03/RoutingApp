using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Diagnostics;

namespace RoutingApp
{
    class Routes
    {
        public Routes(double[,] mtx_)
        {
            mtx = mtx_;
            len = mtx.GetLength(0);

            populateNodes();
            foreach (Node n in ndAll) routeNode(n);
        }

        double[,] mtx;

        int len;
        List<Rt> routes = new List<Rt>();
        List<Node> ndAll = new List<Node>();

        class Rt
        {
            public Node head;
            public int len;
            public Rt(Node head_)
            {
                head = head_;
                head.master = this;
                len = 1;
            }

            public List<int> getRouteList()
            {
                List<int> rt = new List<int>();
                Node current = head;
                while (current != null)
                {
                    rt.Add(current.id);
                    current = current.next;
                }
                return rt;
            }
        }

        void deleteNode(Node n) //deletes node and all nodes connected after it
        {
            if (n.next != null) deleteNode(n.next);
            ndAll.Remove(n);
            return;
        }

        public List<List<int>> getRoutes()
        {
            List<List<int>> rts = new List<List<int>>();
            foreach (Rt r in routes)
            {
                List<int> rt = r.getRouteList();
                rts.Add(rt);
            }
            return rts;
        }

        void updateNodes(double[,] mtx_, Dictionary<int,int> idMap) //maps old id to new id
        {
            mtx = mtx_;
            foreach (Node n in ndAll) n.id = idMap[n.id];
            List<Node> newNodes = populateNodes();
            foreach (Node n in newNodes) routeNode(n);
        }

        List<Node> populateNodes()
        {
            List<Node> newNodes = new List<Node>();
            for (int i = (ndAll == null)? 1 : ndAll.Count(); i < mtx.GetLength(0); i++)
            {
                Node foo = new Node(i);
                ndAll.Add(foo);
                newNodes.Add(foo);
            }
            return newNodes;
        }

        public List<int> deleteRoute(int r)
        {
            List<int> rt = routes[r].getRouteList();
            deleteNode(routes[r].head);
            routes.RemoveAt(r);
            return rt;
        }

        void insertNode(Node aNode, Node bNode, Node cNode) //insert b between a and c
        {
            if (bNode.master != null) removeNode(bNode, true);

            if (aNode == null && cNode == null)
            {
                routes.Add(new Rt(bNode));
                return;
            }
            if (aNode == null)//only cNode exists
            {
                bNode.master = cNode.master;
                bNode.master.head = bNode;
                bNode.next = cNode;
                cNode.prev = bNode;
                bNode.master.len++;
                return;
            }
            //aNode exists
            bNode.master = aNode.master;
            aNode.next = bNode;
            bNode.prev = aNode;
            bNode.master.len++;
            if (cNode != null)
            {
                bNode.next = cNode;
                cNode.prev = bNode;
            }
        }

        void removeNode(Node n, bool eval = false)
        {
            if (n.master == null) return;
            if(n.next == null && n.prev == null)
            {
                routes.Remove(n.master);
                n.master = null;
                return;
            }
            if (n.prev == null) n.master.head = n.next;
            else n.prev.next = n.next;

            if (n.next != null) n.next.prev = n.prev;

            n.master.len--;
            n.eff = 0;
        }

        class Node
        {
            public Node(int id_)
            {
                id = id_;
                if(id == 0)
                {
                    Console.Write(this);
                }
            }
            public Node next;
            public Node prev;
            public Rt master;
            public int id;
            public double eff = 0;

            static public int Comparer(Node x, Node y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                if (x.eff == y.eff) return 0;
                return (x.eff - y.eff) > 0 ? 1 : -1;
            }
        }

        double Gdt(int b) { return mtx[0, b] + mtx[b, 0]; } //gross change in time

        double Pdt(int a, int b, int c) { return mtx[a, b] + mtx[b, c] - mtx[a, c]; } //path change in time

        void evalNode(Node aNode, Node bNode, Node cNode)
        {
            int a = (aNode == null) ? 0 : aNode.id;
            int b = bNode.id;
            int c = (cNode == null) ? 0 : cNode.id;

            int L = 0;
            if (aNode == null && cNode == null) L = 1;
            if (aNode != null) L = aNode.master.len;
            else if (cNode != null) L = cNode.master.len;

            double g = Gdt(b);
            double p = Pdt(a, b, c);
            double path = (g - p) / g;

            double x = .5;//bigger value causes greater variability
            double y = .85;//bigger value favors shorter routes
            double d = getDriverAvailability();
            double q = ndAll.Count();
            double driv = 1 / (x * Math.Abs((q - d * y * L) / d) + 1);

            Debug.WriteLine(driv.ToString("0.##") + ", " + L + ", " + path);

            double eff = .5 * path + .5 * driv;

            bNode.eff = eff;
        }

        void evalNode(Node n)
        {
            if (n != null) evalNode(n.prev, n, n.next);
        }

        double getDriverAvailability()
        {
            return 12;
        }

        public void rerouteNodes(int n)
        {
            List<Node> ndEff = new List<Node>();
            foreach (Rt r in routes)
            {
                Node current = r.head;
                while (current != null)
                {
                    if (current.eff != 0) ndEff.Add(current);
                    current = current.next;
                }
            }

            if (n > ndEff.Count()) n = ndEff.Count();
            for (int i = 0; i < n; i++)
            {
                ndEff.Sort(Node.Comparer);
                routeNode(ndEff[0]);
            }
        }

        void routeNode(Node n)
        {
            Node nBestA = null;
            Node nBestC = null;
            double eBest = 0;
            double eMin = .6;

            removeNode(n);
            foreach (Rt r in routes)
            {
                Node current = r.head;
                while (current != null)
                {
                    evalNode(current, n, current.next);
                    if (n.eff > eMin && n.eff > eBest)
                    {
                        eBest = n.eff;
                        nBestA = current;
                        nBestC = current.next;
                    }
                    current = current.next;
                }
            }
            insertNode(nBestA, n, nBestC);
            evalNode(n);
            evalNode(nBestA);
            evalNode(nBestC);
        }

    }
}
