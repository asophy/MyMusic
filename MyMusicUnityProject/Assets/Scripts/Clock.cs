using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[Serializable]
public class Clock 
{
    public double BPM = 120;


    [SerializeField]
    private Phase BarPhase;
    [SerializeField]
    private int currentSample = 0;
    [SerializeField]
    private double currentDSPTime = 0;
    [SerializeField]
    private double currentBarPos = 0;
    private float[] barPos;


    [SerializeField]
    private bool isplaying;
    [SerializeField]
    private int samplerate;
    public int SampleRate { get { return samplerate; } }
    //public MeshRenderer renderer;

    public double barPhase
    {
        get
        {
            return currentBarPos - ((int)currentBarPos);
        }
    }
    public double GetBeat(double beatPosition, int beat = 4)
    {

        return beatPosition / (1.0 / beat);
    }
    public static double Phase(double barPosition,double length)
    {
        ;

        return (barPosition % length )/ length;
    }
    public static double Beat4th(double beatPosition)
    {
        return (beatPosition % (1.0 / 4)) / (1.0 / 4);
    }
    public static double Beat16th(double beatPosition)
    {
        return (beatPosition % (1.0 / 16)) / (1.0 / 16);
    }
    public static double Beat64th(double beatPosition)
    {
            return (beatPosition % (1.0 / 64)) / (1.0 / 64);
    }

    public double beatLength
    { get {
            return 1.0/(BPM / 60.0) ;
        }
    }
    private double sampleLength {
        get { return 1.0 / samplerate; }
    }
    private double timeToBar( double time)
    {
        return time / (beatLength * 4);
    }

    public void Reset()
    {

        currentBarPos = 0;

    }
    public void Reset(int newPosition)
    {
        var diffSmpl = newPosition - currentSample;
        var diffSec = sampleLength * diffSmpl;
        var diffBar = timeToBar(diffSmpl);

        currentBarPos += diffBar;
        currentDSPTime += diffSec;
        currentSample += diffSmpl;

    }
    // Start is called before the first frame update
    public Clock(int sampleRate)
    {
        //renderer = GetComponent<MeshRenderer>();
        samplerate = sampleRate;
        BarPhase = new Phase();
    }
    public double[] readCache()
    {
        //var data = new float[cache.Length];
        //cache.CopyTo(data, 0);

        return cache;
    }
    public double readCache(int i)
    {
        //var data = new float[cache.Length];
        //cache.CopyTo(data, 0);

        return cache[i];
    }
    double[] cache;
    public double[] Read( AudioContext ac)
    {

        samplerate = ac.SampleRate;
        var data = new double[ac.length ];
        cache = new double[ac.length];
        if (sampleLength == 0.0)
            return data;



        float[] barphase = BarPhase.Read((  BPM/ 60.0)/4.0, ac);




        double sl = sampleLength;

        for (int n = 0; n < data.Length; n += ac.channels)
        {
            currentSample++;
            currentDSPTime += sl;
            currentBarPos += timeToBar(sl);
            for (int i = 0; i < ac.channels; i++)
            {
                data[n + i] = currentBarPos;
            }
        }
        //Update();
        data.CopyTo(cache,0);
        return data;

    }



    int bar;
    private int b4;
    private int b16;
    private int b64;
    public int Bar { get { return (int)currentBarPos; } }
    public int B4 { get { return (int)Beat4th(barPhase); } }
    public int B16 { get { return (int)Beat16th(barPhase); } }
    public int B64 { get { return (int)Beat64th(barPhase); } }
    public float BarPosition { get { return (float)currentBarPos; } }
    public float PBar { get { return pbar = (float)(currentBarPos - bar); } }
    public float P4 { get { return p4 = (float)(Beat4th(barPhase) - b4); } }
    public float P16 { get { return p16 = (float)(Beat16th(barPhase) - b16); } }
    public float P64 { get { return p64 = (float)(Beat64th(barPhase) - b64); } }


    private float pbar;
    private float p4;
    private float p16;
    private float p64;
}
