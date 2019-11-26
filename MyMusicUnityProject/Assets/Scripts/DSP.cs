using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;
//using MathNet.Symbolics;
//using MathNet.Filtering;
//using Expr = MathNet.Symbolics.SymbolicExpression;

using Sample = System.Double;

public class Note:IPatternable
{
    public int noteNum = 0;
    public bool on = false;
    public static int Octave = 16;
    public double velocity = 0;
    public double length = 0;
    public Note(double length, int octave, int noteNum, bool on = true, double velocity = 1):this(length,Oct(octave, noteNum), on,velocity)
    {

    }
    public Note(double length, int noteNum, bool on = true, double velocity = 1)
    {
        this.noteNum = noteNum;
        this.on = on;
        this.velocity = velocity;
        this.length = length;
    }
    public static int Oct(int oct , int noteNum)
    {
        return oct * Octave +noteNum;
    }
    public static int
        A = 12,
        B = 15,
        C = 1,
        D = 3,
        E = 5,
        F = 6,
        G = 9
        ;
    public static double ScaleToFreq(double scale, AudioContext ac)
    {

        var c = ac.baseFreq / (26.0 / 16.0);// A to C

        return scale * c;
    }
    public static double NotenumToScale(int note, AudioContext ac)
    {
        if (note == 0)
        {
            return 0;
        }
        var scale = new double[] {
             16.0 / 16.0, // C    1 
             17.0 / 16.0, // C# 2
             18.0 / 16.0, // D   3
             19.0 / 16.0, // D#
             20.0 / 16.0, // E 3rd 5
             21.0 / 16.0, // F 4th 6
             22.0 / 16.0, // F#
             23.0 / 16.0, // Gb
            
             24.0 / 16.0, // G  9
             25.0 / 16.0, // G#
             26.0 / 16.0, // Ab

             27.0 / 16.0, // A  12
             28.0 / 16.0, // A#
             29.0 / 16.0, // Bb
             30.0 / 16.0, // B 15
             31.0 / 16.0, // B#


        };
        var oct = -3 + (note - 1) / scale.Length;

        return Mathf.Pow((float)2, oct) * scale[(note - 1) % scale.Length];
    }
    public static double NotenumToFreq(int note, AudioContext ac)
    {
        if (note == 0)
        {
            return 0;
        }
        var scale = new double[] {
             16.0 / 16.0, // C    1 
             17.0 / 16.0, // C# 2
             18.0 / 16.0, // D   3
             19.0 / 16.0, // D#
             20.0 / 16.0, // E 3rd 5
             21.0 / 16.0, // F 4th 6
             22.0 / 16.0, // F#
             23.0 / 16.0, // Gb
            
             24.0 / 16.0, // G  9
             25.0 / 16.0, // G#
             26.0 / 16.0, // Ab

             27.0 / 16.0, // A  12
             28.0 / 16.0, // A#
             29.0 / 16.0, // Bb
             30.0 / 16.0, // B 15
             31.0 / 16.0, // B#


        };
        var oct = -3 + (note - 1) / scale.Length;

        return Mathf.Pow((float)2, oct) * scale[(note - 1) % scale.Length];
    }

    double IPatternable.length()
    {
        return length;
    }
}
public class Code: IPatternable
{
    public int[] noteNums ;
    public bool on = false;
    public static int Octave = 16;
    public double velocity = 0;
    public double length = 0;

    public Code(double length, int[] noteNums, bool on = true, double velocity = 1)
    {
        this.noteNums = new int[noteNums.Length];
        noteNums.CopyTo(this.noteNums,0);
        this.on = on;
        this.velocity = velocity;
        this.length = length;
    }
    double IPatternable.length()
    {
        return length;
    }

}

public class DSPPlayable : PlayableBehaviour
{
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        float blend = Mathf.PingPong((float)playable.GetTime(), 1.0f);


        base.PrepareFrame(playable, info);
    }
    public void ProcessFrame(Playable playable, FrameData info, object playerData) {
    }
}
public struct Complex
{
    public double re;
    public double im;
    public Complex(double re, double im)
    {
        this.re = re;
        this.im = im;
    }
    public static double arg(Complex a)
    {

        return Math.Atan(a.im/a.re);
    }

    public static double Length(Complex a)
    {
        return Math.Sqrt(a.re * a.re + a.im * a.im);
    }
    public static implicit operator Complex(double a)
    {
        return new Complex(a,0);
    }
    public static Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.re + b.re, a.im + b.im);
    }
    public static Complex operator -(Complex a, Complex b)
    {
        return new Complex(a.re - b.re, a.im - b.im);
    }
    public static Complex operator *(Complex a, Complex b)
    {
        return new Complex(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re);
    }

}
public struct ComplExp
{
    Complex val;
    public ComplExp(Complex val)
    {
        this.val = val;
    }
    public Complex Eval()
    {
        var ex = Math.Pow(Math.E, this.val.re);
        return  new Complex(ex * Math.Cos(val.im) , ex * Math.Sin(val.im));
    }
    public static ComplExp operator *(ComplExp a, ComplExp b)
    {
        var ac = a.val;
        var bc = b.val;

        return new ComplExp(ac + bc);
    }
    public static ComplExp SinWave( double Amp, double freq, double initialPhase, double time) {

        return new ComplExp(
            new Complex(Amp ,(freq * time + initialPhase)))
            ;

    }
}

public class DSP 
{
    public double radian(double norm)
    {
        return 2 * Math.PI * norm;
    }
    public double Omega(double freq)
    {
        return radian(freq);
    }
    public static float Saw(float phase, AudioContext ac)
    {

        return (float)(1- phase * 2.0 - 2);
    }
    public static float Sine(float phase, AudioContext ac)
    {

        return Mathf.Sin((float)(phase * 2.0f * Math.PI));
    }
    public delegate float Function(float fphase, AudioContext ac);

    // -1 to 1
    public static float NormToFreq(float one, AudioContext ac)
    {
        var volt = one * 5;

        return VoltToFreq(volt, ac);
    }


    // -5 to 5
    public static float VoltToFreq(float volt, AudioContext ac)
    {
        var ere = (float)ac.baseFreq * Mathf.Pow(volt, 2.0f);

        return ere;
    }

    public static double Env(double atack, double decay, double snappiness, double phase, AudioContext ac) {


        var total = (atack + decay);
        var at = atack/total;
        var dt = 1 - at;
        double res = 0;
        if (phase < total)
        {
            if (phase < atack)
            {
                res = (phase / atack);
            }
            else
            {
                res = (1 - (phase - atack) / decay);
            }
        }
        else {
            res = 0;
        }
        return Math.Pow( res, snappiness);
    }
    private static float[] noiseTable;
    public static float noise(float phase)
    {
        var p = phase % 1.0f;
        return noiseTable[(int)(noiseTable.Length * p)];
    }

    public static void init()
    {
        if (noiseTable == null)
        {
            noiseTable = new float[1000 * 1000 * 10];
            for (int i = 0; i < noiseTable.Length; i++)
            {
                var v = Random.Range(0f, 1f);
                noiseTable[i] = v;
            }
        }
    }
}

[Serializable]
public class AudioContext
{
    public double baseFreq = 440.0;
    public int SampleRate { get { return clock.SampleRate; } }
    public int channels;
    public int length;
    public Clock clock;

    public AudioContext(int sampleRate,int length = 2048, int channels=2)
    {
        this.clock = new Clock(sampleRate);
        this.length = length;
        this.channels = channels;

    }
}

public interface IModule
{
     float Read( AudioContext ac);

}

[Serializable]
public class Phase
{
    [SerializeField]
    private double phase = 0;


    public float[] Read(double freq, AudioContext ac)
    {
        int length = ac.length;
        int channels = ac.channels;
        float[] data = new float[length];
        for (int n = 0; n < length; n += channels)
        {
            var p = read(freq, ac);
            for (int i = 0; i < channels; i++)
                data[n + i] = (float)p;


        }
        return data;
    }
    public double read(double freq, AudioContext ac)
    {
        double sampleRate = ac.SampleRate;
        double f = freq / sampleRate;
        phase += f;
        phase -= Mathf.Floor((float)phase);



        return phase;
    }
}

public class RingBuffer {
    double[] buf;
    int offset;

    public RingBuffer(int length = 48000)
    {
        buf = new double[length];
        offset = 0;

    }


    public double this[int n]
    {
        get { return this.buf[offset + n % buf.Length]; }
        set { this.buf[offset + n % buf.Length] = value; }
    }

    public void Insert(double x)
    {
        this.buf[offset] = x;
        
        
        offset++;
        if (offset >= buf.Length)
        {
            offset = 0;
        }

    }





}



[Serializable]
public class BD
{



    [SerializeField]
    private Phase oscPh;


    public BD()
    {
        oscPh = new Phase();

    }


    public Vector4[] Read( AudioContext ac)
    {

        var data = new Vector4[ac.length];
        //float[] beatphase = beatPh.Read(ac.clock.BPM/60.0,ac);
        //float[] oscphase = oscPh.Read((double)freq, ac);



        for (int n = 0; n < data.Length; n += ac.channels)
        {
            //float[] barphase = phase;
            float loopphase = (float)Clock.Phase(ac.clock.BarPosition, 1.0);
            var phase = ac.clock.readCache(n);

            float beatphase = (float)Clock.Beat4th(phase);
            float envA = (float)DSP.Env(0.001f, 0.99f, 1, beatphase, ac);
            float envP = (float)DSP.Env(0.001f,0.8f, 6, beatphase, ac);
            var osc = oscPh.read(0.5*ac.baseFreq * envP, ac);
            // float s = Sine(oscPh.read(freq, ac), ac) * amp * sawsq;
            float o = DSP.Sine((float)osc, ac) * envA;

            for (int i = 0; i < ac.channels; i++)
            {

                data[n + i] = new Vector4(o, beatphase, envA, loopphase);
            }
        }
        return data;
    }

}
public interface IPatternable
{
    double length();
}
public class Pattern
{
    static  public List<int> NoteToNum(List< Note> ns)

    {
        var ptn = new List<int>();
        foreach(var n in ns)
        {
            ptn.Add(n.noteNum);

        }


        return ptn;
    }
    public class PatternResult<T>
    {
        public T note;
        public double phase;

    }
    public static double Length<T>(IEnumerable<T> ptn) where T: IPatternable
    {
        return ptn.Sum(n => n.length());
    }
        public static PatternResult<T> Read<T>(IEnumerable<T> ptn, double barpos) where T : IPatternable
    {

        var loopLength = Length(ptn);
        double loopPos = barpos % loopLength;
        //double loopphase = loopPos / loopLength;


        PatternResult<T> res = new PatternResult<T>() { note = ptn.First(), phase = 0 };
        double start = 0;
        foreach (var n in ptn)
        {
            double end = start + n.length();

            if (loopPos > start && loopPos < end)
            {

                var p = (loopPos - start) / n.length();
                res = new PatternResult<T>(){note=n,phase=p } ;
            }

            start = end;

        }
        return res;

    }

    public static Vector2 Read(int[] ptn, float stepLength, float barpos, AudioContext ac)
    {

        if (ptn == null)
        {
            return new Vector2();
        }
        var stepNum = (int)ptn.Length;

        double loopLength = stepNum * stepLength;
        double loopphase = Clock.Phase(barpos, loopLength);
        double loopPos = loopphase * loopLength;
        int step = (int)((loopPos) / stepLength);
        var pitch = ptn[step];

        //float loopPhase = loopPos / loopLength;

        var stepPhase = (loopPos % stepLength) / stepLength;
        //var trig = Mathf.Clamp(pitch, 0,1);
        var trigPhase = stepPhase;
        if (loopPos % stepLength == 0)
        {
            trigPhase = stepPhase;

        }

        return new Vector2(pitch, (float) trigPhase);
    }
}


[Serializable]
public class Hi
{



    [SerializeField]
    private Phase oscPh;

    public Hi()
    {
        oscPh = new Phase();
    }

    public Vector4[] Read( AudioContext ac)
    {


        Vector4[] data = new Vector4[ac.length];
        //float[] beatphase = beatPh.Read(ac.clock.BPM/60.0,ac);
        //float[] oscphase = oscPh.Read((double)freq, ac);



        for (int n = 0; n < data.Length; n += ac.channels)
        {
            var ptn = new int[] {1,1,0,1,0,1,0,1, 0, 1, 0, 1, 0, 1, 0, 1 };
            float barpos = (float)ac.clock.readCache(n);
            float loopPhase = barpos%(ptn.Length * 1.0f / 8);
            var ptnphase = Pattern.Read(ptn, 1.0f / 8, barpos, ac);
            var trigP = ptnphase.y;

            float envA = (float)DSP.Env(0.001f, 0.1f, 1, trigP, ac);
            float envP = (float)DSP.Env(0.001f, 0.99f, 9, trigP, ac);
            var osc = oscPh.read(ac.baseFreq , ac);
            // float s = Sine(oscPh.read(freq, ac), ac) * amp * sawsq;
            float o = DSP.noise((float)osc ) * envA;

            for (int i = 0; i < ac.channels; i++)
            {
                data[n + i] = new Vector4( o, trigP, envA, loopPhase) ;
            }
        }
        return data;
    }

}






[Serializable]
public class OSC
    {
    
    [SerializeField]
    private Phase oscPh;
    public OSC()
    {
        oscPh = new Phase();
    }

    public float Read(Func<float, AudioContext, float> f, IEnumerable<float> freqs, AudioContext ac)
    {
        float count = freqs.Count();
        float val = 0;
        float baseFreq = freqs.First();
        var phase = (float)oscPh.read(baseFreq, ac);
        foreach (var freq in freqs)
        {
            val += f(phase * freq, ac) / count;
        }
        return val;
    }

    public float Read(Func<float, AudioContext, float> f, float freq, AudioContext ac)
    {
        var phase = oscPh.read(freq, ac);
        float val = f((float)phase, ac);
        return val;
    }
    public float Sine(float phase, AudioContext ac)
    {
        return Read(DSP.Sine, phase, ac);
    }
    public float Saw(float phase, AudioContext ac)
    {
        return Read(DSP.Saw, phase, ac);
    }

}

//from
//http://www.musicdsp.org/en/latest/Filters/38-lp-and-hp-filter.html
[Serializable]
public class Lowpass {
    [Range(0, 1)]
    public double reso = 0.3f;
    [Range(10, 20000)]
    public double cutoff = 200;
    public double c;
    public double a1;
    public double a2;
    public double a3;
    public double b1;
    public double b2;
    public double in0;
    public double in1;
    public double in2;

    public double out0;
    public double out1;
    public double out2;
    public double sample_rate = 48000;



    public Lowpass()
    {
        in0 = 0;
        in1 = 0;
        in2 = 0;
        out0 = 0;
        out1 = 0;
        out2 = 0;


    }
    public double read(double inval, double cutoff, double reso,AudioContext ac)
    {

        var sample_rate = ac.SampleRate;
        var r = Math.Sqrt(2) * (1.01 - reso);

        var f = (sample_rate/2.0) * (cutoff) + 10;

        c = 1.0 / Math.Tan(Math.PI * f / sample_rate);

        a1 = 1.0 / (1.0 + r * c + c * c);
        a2 = 2 * a1;
        a3 = a1;
        b1 = 2.0 * (1.0 - c * c) * a1;
        b2 = (1.0 - r * c + c * c) * a1;

        in2 = in1;
        in1 = in0;
        in0 = inval;

        out2 = out1;
        out1 = out0;
        out0 = a1 * in0 + a2 * in1 + a3 * in2 - b1 * out1 - b2 * out2;




        return out0;

    }

}



[Serializable]
public class Bass
{



    [SerializeField]
    private OSC osc;

    [SerializeField]
    private Lowpass filter;

    [Range(0.00001f, 1)]
    public double test;
    [Range(0.00001f, 1)]
    public double test2;
    [Range(0.00001f, 1)]
    public double test3;
    public Bass()
    {
        osc = new OSC();
        filter = new Lowpass();
    }


    public float Read(float freq, AudioContext ac)
    { 
        var val = osc.Read(DSP.Saw,freq, ac);
        return val;
    }
    public Vector4[] Read(List<Note> ptn, float stepLength , Vector4[] bd, AudioContext ac)
    {



        Vector4[] data = new Vector4[ac.length];
        //float[] beatphase = beatPh.Read(ac.clock.BPM/60.0,ac);
        //float[] oscphase = oscPh.Read((double)freq, ac);



        for (int n = 0; n < data.Length; n += ac.channels)
        {


            float barpos = (float)ac.clock.readCache(n);

            float loopphase = (float)Clock.Phase(barpos, 2.0);



            var ptnphase = Pattern.Read(ptn, barpos);
            var trigP = ptnphase.phase;
            var note = ptnphase.note;
            var scale = Note.NotenumToScale(note.noteNum, ac);
            var pitch = Note.ScaleToFreq(scale, ac);

            float envA = (float)DSP.Env(0.001f, 0.999f, 1, trigP, ac);
            float envF = (float)DSP.Env(0.001f, 0.4f, 2, trigP, ac) * 0.7f + 0.01f;
             envA *= (1 - bd[n].z);//side chain

            var osc = Read((float)pitch, ac);
            var fed = filter.read(osc, envF,0.8,ac);
            // float s = Sine(oscPh.read(freq, ac), ac) * amp * sawsq;
            float o = ((float)fed) * envA;

            for (int i = 0; i < ac.channels; i++)
            {
                data[n + i] = new Vector4(o, envA, (float)scale, loopphase);
            }
        }
        return data;
    }

}


[Serializable]
public class PadSingle
{



    [SerializeField]
    private OSC osc;

    public PadSingle()
    {
        osc = new OSC();
    }


    public float Read(float freq, float trigerPhase, AudioContext ac)
    {
        var val = osc.Read(DSP.Sine, freq, ac) ;
        return val;
    }
    public Vector4[] Read(List<Note> ptn, float stepLength,   AudioContext ac)
    {



        Vector4[] data = new Vector4[ac.length];
        //float[] beatphase = beatPh.Read(ac.clock.BPM/60.0,ac);
        //float[] oscphase = oscPh.Read((double)freq, ac);



        for (int n = 0; n < data.Length; n += ac.channels)
        {




            float barpos = (float)ac.clock.readCache(n);
            float loopphase = (float)Clock.Phase(barpos, 2.0);



            var ptnphase = Pattern.Read(ptn,  barpos);
            var trigP = ptnphase.phase;
            var note = ptnphase.note;
            var scale = Note.NotenumToScale(note.noteNum, ac);
            var pitch = Note.ScaleToFreq(scale, ac);


            var osc = Read((float)pitch, (float)trigP, ac);
            // float s = Sine(oscPh.read(freq, ac), ac) * amp * sawsq;
            float envA = (float)DSP.Env(0.01f, 0.99f, 1, trigP, ac);
            float o = ((float)osc)* (envA);

            for (int i = 0; i < ac.channels; i++)
            {
                data[n + i] = new Vector4(o, envA, (float)pitch, loopphase);
            }
        }
        return data;
    }

}
[Serializable]
public class HarmoPad
{



    [SerializeField]
    private List<OSC> oscs;
    private OSC osc;

    public HarmoPad()
    {
        osc = new OSC();
        oscs = new List<OSC>();
        oscs.Add(  new OSC());
    }

    public float Read(IEnumerable< float> freqs, AudioContext ac)
    {
        float val =0;
        var s = freqs.Zip(oscs, (freq, osc) => new { freq, osc });
        float count = s.Count();
        foreach (var f in s)
        {

            val += f.osc.Read(DSP.Sine, f.freq, ac) / count;
        }


        return val;
    }

    public Vector4[] Read( List<Code> ptn, float stepLength,  AudioContext ac)
    {
        Vector4[] data = new Vector4[ac.length];
        //float[] beatphase = beatPh.Read(ac.clock.BPM/60.0,ac);
        //float[] oscphase = oscPh.Read((double)freq, ac);



        //var nptns = new List<Note[]>();
        //var ptns = nptns.ConvertAll(new Converter<Note[], int[]>(Pattern.NoteToNum));




        for (int n = 0; n < data.Length; n += ac.channels)
        {
            for (int c = 0; c < ac.channels; c++)
            {
                float barpos = (float) ac.clock.readCache(n);
                float loopphase = (float)Clock.Phase(barpos, stepLength);

                var outd = Pattern.Read(ptn, barpos);




                var trigP = outd.phase;
                var code = outd.note;
                if (code.noteNums.Length > oscs.Count)
                {
                    for (int i = 0; i < code.noteNums.Length - oscs.Count; i++)
                    {
                        oscs.Add(new OSC());
                    }
                }
                float envA = (float)DSP.Env(0.2f, 0.8f, 1, trigP, ac);

                float val = 0;
                int count = 0;

                var notes = from note in code.noteNums
                            select new { 
                                scale =  Note.NotenumToScale(note, ac),
                                freq = Note.ScaleToFreq(Note.NotenumToScale(note,ac), ac) } ;
                var freqs = notes.Select(note => (float)note.freq);
                var scales = notes.Select(note => note.scale);

                val = Read(freqs, ac)* envA;



                data[n + c] = new Vector4(val, envA, 0, loopphase);


            }








        }
        return data;
    }

}

