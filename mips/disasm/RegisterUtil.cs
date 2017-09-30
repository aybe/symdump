﻿using System;

namespace mips.disasm
{
    public static class RegisterUtil
    {
        public static int ToInt(Register r)
        {
            return (int) r;
        }

        public static int ToInt(C0Register r)
        {
            return ToInt(Register.Sentinel) + (int) r;
        }

        public static int ToInt(C2Register r)
        {
            return ToInt(C0Register.Sentinel) + (int) r;
        }

        public static Register? RegisterFromInt(int i)
        {
            if (i < 0 || i >= (int) Register.Sentinel)
                return null;

            return (Register) i;
        }

        public static C0Register? C0RegisterFromInt(int i)
        {
            i -= (int) Register.Sentinel;

            if (i < 0 || i >= (int) C0Register.Sentinel)
                return null;

            return (C0Register) i;
        }

        public static C2Register? C2RegisterFromInt(int i)
        {
            i -= (int) Register.Sentinel + (int) C0Register.Sentinel;

            if (i < 0 || i >= (int) C2Register.Sentinel)
                return null;

            return (C2Register) i;
        }

        public static string RegisterStringFromInt(int registerId)
        {
            var tmp = RegisterFromInt(registerId);
            if (tmp != null)
                return tmp.Value.ToString();

            var tmp2 = C0RegisterFromInt(registerId);
            if (tmp2 != null)
                return tmp2.Value.ToString();

            var tmp3 = C2RegisterFromInt(registerId);
            if (tmp3 != null)
                return tmp3.Value.ToString();

            throw new ArgumentOutOfRangeException(nameof(registerId));
        }
    }
}
