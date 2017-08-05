﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using core.util;
using NLog;

namespace exefile.controlflow
{
    public class Reducer
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public readonly SortedDictionary<uint, IBlock> blocks = new SortedDictionary<uint, IBlock>();

        public Reducer(ControlFlowProcessor processor)
        {
            foreach (var block in processor.blocks)
            {
                blocks.Add(block.Key, block.Value);
            }
        }

        public void reduce()
        {
            var reduced = false;
            do
            {
                reduced = blocks.Values.Any(reduceSequence)
                          || blocks.Values.Reverse().Any(reduceIf)
                          || blocks.Values.Reverse().Any(reduceIfElse);
            } while (reduced);
        }

        private bool reduceSequence(IBlock block)
        {
            var next = block.trueExit;
            logger.Debug(
                $"SEQ check {block.start:X} {block.start:X}={block.exitType} {next?.start:X}={next?.exitType}");
            if (block.exitType != ExitType.Unconditional ||
                (next?.exitType != ExitType.Unconditional && next?.exitType != ExitType.Return))
                return false;


            // count refs to the next block
            if (blocks.Values.Count(b => b.trueExit?.start == next.start || b.falseExit?.start == next.start) > 1)
                return false;

            if (block is SequenceBlock)
            {
                var existing = (SequenceBlock) block;
                Debug.Assert(existing.trueExit != null);

                logger.Debug($"Sequence {block.start:X}: attach block {next.start:X}");

                existing.sequence.Add(existing.trueExit.start, existing.trueExit);
                blocks.Remove(existing.trueExit.start);
                return true;
            }

            logger.Debug($"New sequence {block.start:X} with block {next.start:X}");

            var seq = new SequenceBlock();
            seq.sequence.Add(block.start, block);
            seq.sequence.Add(next.start, next);
            blocks.Remove(block.start);
            blocks.Remove(next.start);
            blocks.Add(seq.start, seq);
            return true;
        }

        private bool reduceIf(IBlock condition)
        {
            /*
            if(condition<exit=conditional>)
              body<exit=unconditional|return>;
            commonCode;
            */

            if (condition.exitType != ExitType.Conditional)
                return false;

            var common = condition.trueExit;
            Debug.Assert(common != null);
            var body = condition.falseExit;
            Debug.Assert(body != null);

            return tryMakeIfBlock(condition, body, common, true)
                   || tryMakeIfBlock(condition, common, body, false);

            // swap and try again
        }

        private bool reduceIfElse(IBlock condition)
        {
            /*
            if(condition<exit=conditional>) trueBody<exit=unconditional>;
            else falseBody<exit=unconditional>;
            commonCode;
            */

            if (condition.exitType != ExitType.Conditional)
                return false;

            var trueBody = condition.trueExit;
            Debug.Assert(trueBody != null);
            var falseBody = condition.falseExit;
            Debug.Assert(falseBody != null);

            if (trueBody.exitType != ExitType.Unconditional || trueBody.exitType != ExitType.Unconditional)
                return false;

            var common = trueBody.trueExit;
            Debug.Assert(common != null);
            if (common.start != falseBody.start)
                return false;
            
            var compound = new IfElseBlock(condition, trueBody, falseBody, common);
            blocks.Remove(condition.start);
            blocks.Remove(trueBody.start);
            blocks.Remove(falseBody.start);
            blocks.Add(compound.start, compound);

            return true;
        }

        private bool tryMakeIfBlock(IBlock condition, IBlock body, IBlock common, bool inverted)
        {
            Debug.Assert(condition != null);
            Debug.Assert(body != null);
            Debug.Assert(common != null);

            if (body.exitType != ExitType.Return &&
                (body.exitType != ExitType.Unconditional || body.trueExit != common))
                return false;

            logger.Debug($"Reduce: condition={condition.start:X} body={body.start:X} common={common.start:X}");

            var compound = new IfBlock(condition, body, common, inverted);
            blocks.Remove(condition.start);
            if (body.exitType != ExitType.Return)
                blocks.Remove(body.start);
            blocks.Add(compound.start, compound);

            return true;
        }

        public void dump(IndentedTextWriter writer)
        {
            foreach (var block in blocks.Values)
            {
                block.dump(writer);
                writer.WriteLine();
            }
        }
    }
}
