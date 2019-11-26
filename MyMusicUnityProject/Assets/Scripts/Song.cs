using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MeshRenderer))]
public class Song : MonoBehaviour
{
    [Range(60, 240)]
    public double BPM = 120;
    [SerializeField]
    public BD bd;
    [SerializeField]
    public Hi hi;
    [SerializeField]
    public Bass bass;
    [SerializeField]
    public HarmoPad pad;
    [SerializeField]
    public AudioContext ac;


    public Shader Visualizer;

    public AudioClip clip;

    [Range(0, 1)]
    public float tr1 =1;
    [Range(0, 1)]
    public float tr2 = 1;
    [Range(0, 1)]
    public float tr3 = 1;
    [Range(0, 1)]
    public float tr4 = 1;
    [Range(0, 1)]
    public float debug = 0.3f;

    private Material material;

    private Vector4 trv1;
    private Vector4 trv2;
    private Vector4 trv3;
    private Vector4 trv4;
    private List<Note>  bassPtn;
    private List<Code> padPtn;

    void Start()
    {
        Random.InitState(1672);
        DSP.init();

        material = new Material(Visualizer);

        ac = new AudioContext(AudioSettings.outputSampleRate);
        ac.clock.BPM = BPM;
        var l = 1.0f / 8;
        var o = 1;
        bassPtn = new List<Note>() {
            new Note(l,o,Note.C),
            new Note(l,o,Note.C),
            new Note(l,o,Note.G),
            new Note(l,o,Note.G),

            new Note(l,o,Note.C),
            new Note(l,o,Note.C),
            new Note(l,o,Note.G),
            new Note(l,o,Note.G),

            new Note(l,o,Note.C),
            new Note(l,o,Note.C),
            new Note(l,o,Note.G),
            new Note(l,o,Note.G),

            new Note(l,o,Note.C),
            new Note(l,o+1,Note.C),
            new Note(l,o+1,Note.G),
            new Note(l,o+1,Note.F),
            };

        var oct = Note.Octave;
        padPtn = new List<Code>() {
           new Code(2, new int[]{ 2 * oct + Note.C, 2 * oct + Note.E,  2 * oct+ Note.G }),
           new Code(2, new int[]{ 2 * oct + Note.F, 2 * oct + Note.A, 2 * oct + Note.C }),
        };


        //AudioClip myClip = AudioClip.Create("Song", ac.SampleRate * 2 * 8, ac.channels, ac.SampleRate, true, OnAudioRead, OnAudioSetPosition);
        //AudioSource aud = GetComponent<AudioSource>();
        //aud.clip = myClip;
        //aud.Pause();


        bd = new BD();
        hi = new Hi();
        bass = new Bass();
        pad = new HarmoPad();
    }
    private void Update()
    {

    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        var clock = ac.clock;
        var m = material;
        m.SetInt("_Bar", clock.Bar);
        m.SetInt("_B4", clock.B4);
        m.SetInt("_B16", clock.B16);
        m.SetInt("_B64", clock.B64);
        m.SetFloat("_DEBUG", debug);
        m.SetVector("_Pbar", new Vector4(clock.PBar, clock.P4, clock.P16, clock.P64));
        m.SetVector("_Volume", new Vector4(tr1, tr2, tr3, tr4));
        m.SetVector("_Tr1", trv1);
        m.SetVector("_Tr2", trv2);
        m.SetVector("_Tr3", trv3);
        m.SetVector("_Tr4", trv4);


        //if (intensity == 0)
        //{
        //    Graphics.Blit(source, destination);
        //    return;
        //}

        //material.SetFloat("_bwBlend", intensity);
        Graphics.Blit(source, destination, material);
    }
    void OnAudioFilterRead(float[] data, int channels)
    {
        ac.clock.BPM = BPM;

        ac.length = data.Length;
        ac.channels = channels;

        var clkval = ac.clock.Read(ac);

        var tre1val = bd.Read(ac);
        var tre2val = hi.Read(ac);

        var tre3val = bass.Read(bassPtn, 1.0f / 8, tre1val, ac);

        var tre4val = pad.Read(padPtn, 4.0f / 2, ac);


        for (int n = 0; n < data.Length; n += ac.channels)
        {

            for (int i = 0; i < ac.channels; i++)
            {

                data[n + i] += tre1val[n + i].x * tr1;
                data[n + i] += tre2val[n + i].x * tr2;
                data[n + i] += tre3val[n + i].x * tr3;
                data[n + i] += tre4val[n + i].x * tr4;
                data[n + i] *= 1 / 4.0f;
            }
        }
        trv1 = tre1val[tre1val.Length - 1];
        trv2 = tre2val[tre2val.Length - 1];
        trv3 = tre3val[tre3val.Length - 1];
        trv4 = tre4val[tre4val.Length - 1];


        

    }
}
