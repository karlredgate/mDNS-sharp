
MCS_ARGS = /debug+

test: dig.exe appliances.exe listen.exe
	mono dig.exe MBP17-05683.local
	mono dig.exe MBP17-01173.local
	mono dig.exe MBP17-01173
	mono dig.exe MacBuildMachine.local
	mono dig.exe hobson.local
	mono dig.exe T3500ENG001.local
	mono appliances.exe

dig.exe: dig.cs mDNS.dll
	mcs dig.cs $(MCS_ARGS) /target:exe /reference:mDNS

appliances.exe: appliances.cs mDNS.dll
	mcs appliances.cs $(MCS_ARGS) /target:exe /reference:mDNS

listen.exe: listen.cs mDNS.dll
	mcs listen.cs $(MCS_ARGS) /target:exe /reference:mDNS

mDNS.dll: mDNS.cs packet.cs
	mcs $(MCS_ARGS) /target:library /unsafe $^

clean:
	rm *.dll *.exe *.mdb
