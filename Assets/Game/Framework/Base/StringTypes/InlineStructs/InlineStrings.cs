using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MOYV.StringTypes.InlineStructs
{
    public interface ISpanStringType
    {
        public ref char this[int index] { get; }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        public char[] ToChars();
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct InlineString64
    {
        public long l0;
        public long l1;
        public long l2;
        public long l3;
        public long l4;
        public long l5;
        public long l6;
        public long l7;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct InlineString128
    {
        public long l0;
        public long l1;
        public long l2;
        public long l3;
        public long l4;
        public long l5;
        public long l6;
        public long l7;
        public long l8;
        public long l9;
        public long l10;
        public long l11;
        public long l12;
        public long l13;
        public long l14;
        public long l15;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct InlineString256
    {
        public long l0;
        public long l1;
        public long l2;
        public long l3;
        public long l4;
        public long l5;
        public long l6;
        public long l7;
        public long l8;
        public long l9;
        public long l10;
        public long l11;
        public long l12;
        public long l13;
        public long l14;
        public long l15;
        public long l16;
        public long l17;
        public long l18;
        public long l19;
        public long l20;
        public long l21;
        public long l22;
        public long l23;
        public long l24;
        public long l25;
        public long l26;
        public long l27;
        public long l28;
        public long l29;
        public long l30;
        public long l31;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct InlineString512
    {
        public long l0;
        public long l1;
        public long l2;
        public long l3;
        public long l4;
        public long l5;
        public long l6;
        public long l7;
        public long l8;
        public long l9;
        public long l10;
        public long l11;
        public long l12;
        public long l13;
        public long l14;
        public long l15;
        public long l16;
        public long l17;
        public long l18;
        public long l19;
        public long l20;
        public long l21;
        public long l22;
        public long l23;
        public long l24;
        public long l25;
        public long l26;
        public long l27;
        public long l28;
        public long l29;
        public long l30;
        public long l31;
        public long l32;
        public long l33;
        public long l34;
        public long l35;
        public long l36;
        public long l37;
        public long l38;
        public long l39;
        public long l40;
        public long l41;
        public long l42;
        public long l43;
        public long l44;
        public long l45;
        public long l46;
        public long l47;
        public long l48;
        public long l49;
        public long l50;
        public long l51;
        public long l52;
        public long l53;
        public long l54;
        public long l55;
        public long l56;
        public long l57;
        public long l58;
        public long l59;
        public long l60;
        public long l61;
        public long l62;
        public long l63;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct InlineString1024
    {
        public long l0;
        public long l1;
        public long l2;
        public long l3;
        public long l4;
        public long l5;
        public long l6;
        public long l7;
        public long l8;
        public long l9;
        public long l10;
        public long l11;
        public long l12;
        public long l13;
        public long l14;
        public long l15;
        public long l16;
        public long l17;
        public long l18;
        public long l19;
        public long l20;
        public long l21;
        public long l22;
        public long l23;
        public long l24;
        public long l25;
        public long l26;
        public long l27;
        public long l28;
        public long l29;
        public long l30;
        public long l31;
        public long l32;
        public long l33;
        public long l34;
        public long l35;
        public long l36;
        public long l37;
        public long l38;
        public long l39;
        public long l40;
        public long l41;
        public long l42;
        public long l43;
        public long l44;
        public long l45;
        public long l46;
        public long l47;
        public long l48;
        public long l49;
        public long l50;
        public long l51;
        public long l52;
        public long l53;
        public long l54;
        public long l55;
        public long l56;
        public long l57;
        public long l58;
        public long l59;
        public long l60;
        public long l61;
        public long l62;
        public long l63;
        public long l64;
        public long l65;
        public long l66;
        public long l67;
        public long l68;
        public long l69;
        public long l70;
        public long l71;
        public long l72;
        public long l73;
        public long l74;
        public long l75;
        public long l76;
        public long l77;
        public long l78;
        public long l79;
        public long l80;
        public long l81;
        public long l82;
        public long l83;
        public long l84;
        public long l85;
        public long l86;
        public long l87;
        public long l88;
        public long l89;
        public long l90;
        public long l91;
        public long l92;
        public long l93;
        public long l94;
        public long l95;
        public long l96;
        public long l97;
        public long l98;
        public long l99;
        public long l100;
        public long l101;
        public long l102;
        public long l103;
        public long l104;
        public long l105;
        public long l106;
        public long l107;
        public long l108;
        public long l109;
        public long l110;
        public long l111;
        public long l112;
        public long l113;
        public long l114;
        public long l115;
        public long l116;
        public long l117;
        public long l118;
        public long l119;
        public long l120;
        public long l121;
        public long l122;
        public long l123;
        public long l124;
        public long l125;
        public long l126;
        public long l127;
    }
}