using System;
using System.IO;
using UnityEngine;

public class G
{
    public static readonly WaitForSeconds QuarterSeconds = new WaitForSeconds(0.25f);
    public static readonly WaitForSeconds HalfSeconds = new WaitForSeconds(0.5f);
    public static readonly WaitForSeconds OneSecond = new WaitForSeconds(1f);
    public static readonly WaitForSeconds TwoSeconds = new WaitForSeconds(2f);
    public static readonly WaitForSeconds ThreeSeconds = new WaitForSeconds(3f);
    public static readonly YieldInstruction FiveSeconds = new WaitForSeconds(5);
    public static readonly YieldInstruction TenSeconds = new WaitForSeconds(10);
}