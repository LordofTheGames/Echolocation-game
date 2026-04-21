<CsoundSynthesizer>
<CsOptions>
-odac
</CsOptions>
<CsInstruments>
sr = 44100
ksmps = 32
nchnls = 2
0dbfs = 1.0

instr 1 
    ; Read directly from the p-fields passed by the C# string
    kDelay = p4
    k63    = p5
    k125   = p6
    k250   = p7
    k500   = p8
    k1k    = p9
    k2k    = p10
    k4k    = p11
    k8k    = p12
    k16k   = p13

    ; The dry sound from Unity 
    aDryIn chnget "unity_audio_in" 
    
    ; 1. Spatial Delay based on Ray Distance
    aDelayed delay aDryIn, i(kDelay), 5.0

    ; 2. 9-Band Frequency Attenuation (Bandpass filters)
    aB1 butterbp aDelayed, 63, 30
    aB2 butterbp aDelayed, 125, 60
    aB3 butterbp aDelayed, 250, 125
    aB4 butterbp aDelayed, 500, 250
    aB5 butterbp aDelayed, 1000, 500
    aB6 butterbp aDelayed, 2000, 1000
    aB7 butterbp aDelayed, 4000, 2000
    aB8 butterbp aDelayed, 8000, 4000
    aB9 butterbp aDelayed, 16000, 8000
    
    ; Multiply each band by the energy left after bouncing
    aFilteredMix = (aB1*k63) + (aB2*k125) + (aB3*k250) + (aB4*k500) + (aB5*k1k) + (aB6*k2k) + (aB7*k4k) + (aB8*k8k) + (aB9*k16k)

    ; 3. Schroeder's Reverb
    kReverbTime = kDelay * 10 
    aReverb reverb aFilteredMix, kReverbTime

    ; Output Left and Right
    outs aReverb, aReverb
endin

</CsInstruments>
</CsoundSynthesizer>