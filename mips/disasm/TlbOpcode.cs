﻿using System.Diagnostics.CodeAnalysis;

namespace mips.disasm
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public enum TlbOpcode
    {
        tlbr = 1,
        tlbwi = 2,
        tlbwr = 6,
        tlbp = 8,
        rfe = 16
    }
}
