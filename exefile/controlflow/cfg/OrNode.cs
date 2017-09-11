﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using core;
using core.util;
using JetBrains.Annotations;

namespace exefile.controlflow.cfg
{
    public class OrNode : Node
    {
        private readonly IList<INode> _nodes;

        public override uint Start => _nodes.First().Start;

        public OrNode([NotNull] INode c0) : base(c0.Graph)
        {
            Debug.Assert(c0.Outs.Count() == 2);
            Debug.Assert(c0.Outs.Count(e => e is TrueEdge) == 1);
            Debug.Assert(c0.Outs.Count(e => e is FalseEdge) == 1);

            var c1 = c0.Outs.First(e => e is FalseEdge).To;
            Debug.Assert(!c0.Equals(c1));
            Debug.Assert(c1.Outs.Count() == 2);
            Debug.Assert(c1.Outs.Count(e => e is TrueEdge) == 1);
            Debug.Assert(c1.Outs.Count(e => e is FalseEdge) == 1);

            var sTrue = c0.Outs.First(e => e is TrueEdge).To;
            Debug.Assert(c1.Outs.First(e => e is TrueEdge).To.Equals(sTrue));

            var sFalse = c1.Outs.First(e => e is FalseEdge).To;
            Debug.Assert(!sFalse.Equals(sTrue));
            Debug.Assert(!sFalse.Equals(c0));
            Debug.Assert(!sFalse.Equals(c1));

            if (c0 is OrNode c0Or)
            {
                _nodes = c0Or._nodes;
                _nodes.Add(c1);
            }
            else
            {
                _nodes = new List<INode> {c0, c1};
            }

            Graph.ReplaceNode(c0, this);
            Graph.RemoveNode(c1);
            Graph.AddEdge(new FalseEdge(this, sFalse));
        }

        public static bool IsCandidate([NotNull] INode c0)
        {
            if (c0.Outs.Count() != 2)
                return false;

            if (c0.Outs.Count(e => e is TrueEdge) != 1)
                return false;

            if (c0.Outs.Count(e => e is FalseEdge) != 1)
                return false;

            var c1 = c0.Outs.First(e => e is FalseEdge).To;
            if (c1.Outs.Count() != 2)
                return false;
            if (c1.Outs.Count(e => e is TrueEdge) != 1)
                return false;
            if (c1.Outs.Count(e => e is FalseEdge) != 1)
                return false;

            var sTrue = c0.Outs.First(e => e is TrueEdge).To;
            if (!c1.Outs.First(e => e is TrueEdge).To.Equals(sTrue))
                return false;
            
            var sFalse = c1.Outs.First(e => e is FalseEdge).To;
            if (sFalse.Equals(sTrue))
                return false;

            if (sFalse.Equals(c0))
                return false;
            
            return !sFalse.Equals(c1);
        }

        public override bool ContainsAddress(uint address)
            => _nodes.Any(n => n.ContainsAddress(address));

        public override SortedDictionary<uint, Instruction> Instructions
        {
            get
            {
                var result = new SortedDictionary<uint, Instruction>();
                foreach (var n in _nodes)
                foreach (var insn in n.Instructions)
                    result.Add(insn.Key, insn.Value);
                return result;
            }
        }

        public override void Dump(IndentedTextWriter writer)
        {
            writer.WriteLine("{");
            ++writer.Indent;
            bool first = true;
            foreach (var n in _nodes)
            {
                if (!first)
                {
                    writer.WriteLine("||");
                }
                first = false;

                ++writer.Indent;
                n.Dump(writer);
                --writer.Indent;
            }
            --writer.Indent;
            writer.WriteLine("}");
        }
    }
}
