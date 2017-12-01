del bin\x86\Release\AppX\*.pdb
del bin\x86\Release\AppX\*.appxrecipe

MakeAppx pack /v /d bin\x86\Release\AppX /l /p ..\Build\Release\EarTrumpet_1.5.0.0_x86.appx