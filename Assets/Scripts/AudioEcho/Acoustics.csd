<CsoundSynthesizer>
<CsOptions>
-n -d
</CsOptions>
<CsInstruments>
sr = 44100
ksmps = 32
nchnls = 2
0dbfs = 1.0

instr 1 
    ; 1. Read p-fields. Delay and Energy MUST be i-rate for the delay opcode!
    iDelay = p4
    i63    = p5
    i125   = p6
    i250   = p7
    i500   = p8
    i1k    = p9
    i2k    = p10
    i4k    = p11
    i8k    = p12
    i16k   = p13

    ; 2. Grab the live audio playing from the Unity AudioSource (The dry footstep)
    aInL, aInR ins
    
    ; Mix down to mono for DSP processing
    aDryIn = (aInL + aInR) * 0.5 
    
    ; 3. Spatial Delay (Requires i-rate delay time, 5.0 is max buffer)
    aDelayed delay aDryIn, iDelay, 5.0

    ; 4. 9-Band Frequency Attenuation (Bandpass filters)
    aB1 butterbp aDelayed, 63, 30
    aB2 butterbp aDelayed, 125, 60
    aB3 butterbp aDelayed, 250, 125
    aB4 butterbp aDelayed, 500, 250
    aB5 butterbp aDelayed, 1000, 500
    aB6 butterbp aDelayed, 2000, 1000
    aB7 butterbp aDelayed, 4000, 2000
    aB8 butterbp aDelayed, 8000, 4000
    aB9 butterbp aDelayed, 16000, 8000
    
    ; 5. Multiply each band by the energy left after bouncing
    aFilteredMix = (aB1*i63) + (aB2*i125) + (aB3*i250) + (aB4*i500) + (aB5*i1k) + (aB6*i2k) + (aB7*i4k) + (aB8*i8k) + (aB9*i16k)

    ; 6. Schroeder's Reverb (Scale delay to create reverb tail)
    kReverbTime = iDelay * 10 
    aReverb reverb aFilteredMix, kReverbTime

    ; Output the original dry sound PLUS the new wet reverb
    outs aInL + aReverb, aInR + aReverb
endin

</CsInstruments>
</CsoundSynthesizer>